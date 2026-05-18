using Horr;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using FluentAssertions;
using Entities.Users;
using Entities.Communication;
using Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Entities;

namespace UnitTesting.Integration;

public class ConversationsControllerTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ConversationsControllerTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    private async Task SeedDataAsync(AppDbContext context, string conversationId, string user1Id, string user2Id)
    {
        if (!await context.Users.AnyAsync(u => u.Id == user1Id))
            context.Users.Add(new Entities.Users.User { Id = user1Id, UserName = user1Id, Email = $"{user1Id}@example.com", FullName = user1Id });

        if (!await context.Users.AnyAsync(u => u.Id == user2Id))
            context.Users.Add(new Entities.Users.User { Id = user2Id, UserName = user2Id, Email = $"{user2Id}@example.com", FullName = user2Id });

        if (!await context.Conversations.AnyAsync(c => c.Id == conversationId))
            context.Conversations.Add(new Conversation { Id = conversationId });

        if (!await context.ConversationParticipants.AnyAsync(p => p.ConversationId == conversationId && p.UserId == user1Id))
            context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = conversationId, UserId = user1Id });

        if (!await context.ConversationParticipants.AnyAsync(p => p.ConversationId == conversationId && p.UserId == user2Id))
            context.ConversationParticipants.Add(new ConversationParticipant { ConversationId = conversationId, UserId = user2Id });

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetConversations_Should_Return_UnreadCount_3_For_Other_User_Messages()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = "conv-get-user1";
        var otherUser = "conv-get-user2";
        var conversationId = "conv-get-1";

        await SeedDataAsync(context, conversationId, currentUser, otherUser);

        // 3 Unread from other user
        for (int i = 0; i < 3; i++)
            context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = otherUser, Body = $"Msg {i}", Status = MessageStatus.Unread });

        // 2 Unread from current user
        for (int i = 0; i < 2; i++)
            context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = currentUser, Body = $"My Msg {i}", Status = MessageStatus.Unread });

        await context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, "/api/conversations");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Freelancer");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = result.EnumerateArray().ToList();
        var conv = items.First(c => c.GetProperty("id").GetString() == conversationId);
        conv.GetProperty("unreadCount").GetInt32().Should().Be(3);
    }

    [Fact]
    public async Task GetMessages_Should_Return_Newest_First()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = "msg-ord-user1";
        var otherUser = "msg-ord-user2";
        var conversationId = "conv-ord-1";

        await SeedDataAsync(context, conversationId, currentUser, otherUser);
        
        var now = DateTime.UtcNow;
        context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = otherUser, Body = "Old", SentAt = now.AddHours(-1) });
        context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = otherUser, Body = "New", SentAt = now });
        await context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/conversations/{conversationId}/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Freelancer");

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        var items = result.GetProperty("items").EnumerateArray().ToList();
        items.First().GetProperty("body").GetString().Should().Be("New");
    }

    [Fact]
    public async Task GetMessages_Should_Mark_Other_User_Messages_As_Read()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = "msg-read-user1";
        var otherUser = "msg-read-user2";
        var conversationId = "conv-read-1";

        await SeedDataAsync(context, conversationId, currentUser, otherUser);

        // 3 Unread from other user
        for (int i = 0; i < 3; i++)
            context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = otherUser, Body = $"Other {i}", Status = MessageStatus.Unread });

        // 1 Unread from current user
        context.Messages.Add(new Message { Id = Guid.NewGuid().ToString(), ConversationId = conversationId, SenderId = currentUser, Body = "Mine", Status = MessageStatus.Unread });

        await context.SaveChangesAsync();

        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/conversations/{conversationId}/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Freelancer");

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);

        // Verify DB state
        var otherMsgs = await context.Messages.AsNoTracking().Where(m => m.ConversationId == conversationId && m.SenderId == otherUser).ToListAsync();
        otherMsgs.All(m => m.Status == MessageStatus.Read).Should().BeTrue();

        var myMsg = await context.Messages.AsNoTracking().FirstAsync(m => m.ConversationId == conversationId && m.SenderId == currentUser);
        myMsg.Status.Should().Be(MessageStatus.Unread);
    }

    [Fact]
    public async Task SendMessage_Should_Return_201_And_MessageDto_With_Unread_Status()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = "msg-send-user1";
        var otherUser = "msg-send-user2";
        var conversationId = "conv-send-1";

        await SeedDataAsync(context, conversationId, currentUser, otherUser);

        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Test Message Body"), "body");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/conversations/{conversationId}/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Freelancer");
        request.Content = form;

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        result.GetProperty("body").GetString().Should().Be("Test Message Body");
        result.GetProperty("status").GetString().Should().Be(MessageStatus.Unread.ToString());

    }

    [Fact]
    public async Task GetMessages_UnknownConversationId_Should_Return_404_ProblemDetails()
    {
        var currentUser = "msg-unk-user1";
        
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        context.Users.Add(new Entities.Users.User { Id = currentUser, UserName = currentUser, Email = "unk@e.com", FullName = "Unk" });
        await context.SaveChangesAsync();
        
        var request = new HttpRequestMessage(HttpMethod.Get, $"/api/conversations/unknown-conv/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Freelancer");

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);

        var body = await response.Content.ReadAsStringAsync();
        body.ToLower().Should().Contain("title"); // Verify ProblemDetails shape
    }

    [Fact]
    public async Task SendMessage_Should_InitiateNewConversation_WhenItDoesNotExist_AndFormDataProvided_Integration()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var currentUser = "client-int-sender";
        var otherUser = "freelancer-int-receiver";
        var newConversationId = "conv-new-int-1";
        var jobId = "job-int-123";

        // Seed users
        if (!await context.Users.AnyAsync(u => u.Id == currentUser))
            context.Users.Add(new Entities.Users.User { Id = currentUser, UserName = currentUser, Email = $"{currentUser}@example.com", FullName = currentUser });

        if (!await context.Users.AnyAsync(u => u.Id == otherUser))
            context.Users.Add(new Entities.Users.User { Id = otherUser, UserName = otherUser, Email = $"{otherUser}@example.com", FullName = otherUser });

        // Seed category and job
        if (!await context.Categories.AnyAsync(c => c.Id == "cat-int-1"))
            context.Categories.Add(new Entities.Project.Category { Id = "cat-int-1", Name = "Int Category", Slug = "int-category" });

        if (!await context.JobPosts.AnyAsync(j => j.Id == jobId))
            context.JobPosts.Add(new Entities.Project.JobPost { Id = jobId, Title = "Int Job", Description = "Desc", CategoryId = "cat-int-1", ClientId = currentUser });

        await context.SaveChangesAsync();

        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Dynamic Conversation Initiation Test Message"), "body");
        form.Add(new StringContent(jobId), "jobPostId");
        form.Add(new StringContent(otherUser), "receiverId");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/conversations/{newConversationId}/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Client");
        request.Content = form;

        var response = await _client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        response.StatusCode.Should().Be(HttpStatusCode.Created, body);

        // Verify conversation is created in database
        using var checkScope = _factory.Services.CreateScope();
        var checkContext = checkScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var conversation = await checkContext.Conversations.FirstOrDefaultAsync(c => c.Id == newConversationId);
        conversation.Should().NotBeNull();
        conversation.JobPostId.Should().Be(jobId);

        var participants = await checkContext.ConversationParticipants.Where(p => p.ConversationId == newConversationId).ToListAsync();
        participants.Should().HaveCount(2);
    }

    [Fact]
    public async Task SendMessage_Should_Return_BadRequest_WhenConversationDoesNotExist_AndJobPostNotFound_Integration()
    {
        var currentUser = "client-int-sender-fail";
        var otherUser = "freelancer-int-receiver-fail";
        var newConversationId = "conv-new-int-fail";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        if (!await context.Users.AnyAsync(u => u.Id == currentUser))
            context.Users.Add(new Entities.Users.User { Id = currentUser, UserName = currentUser, Email = $"{currentUser}@example.com", FullName = currentUser });

        if (!await context.Users.AnyAsync(u => u.Id == otherUser))
            context.Users.Add(new Entities.Users.User { Id = otherUser, UserName = otherUser, Email = $"{otherUser}@example.com", FullName = otherUser });

        await context.SaveChangesAsync();

        var form = new MultipartFormDataContent();
        form.Add(new StringContent("Failing Initiation Test Message"), "body");
        form.Add(new StringContent("non-existent-job-id"), "jobPostId");
        form.Add(new StringContent(otherUser), "receiverId");

        var request = new HttpRequestMessage(HttpMethod.Post, $"/api/conversations/{newConversationId}/messages");
        request.Headers.Add("X-Test-UserId", currentUser);
        request.Headers.Add("X-Test-UserRole", "Client");
        request.Content = form;

        var response = await _client.SendAsync(request);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}

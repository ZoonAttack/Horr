# Gemini Service & Recommendation Process Analysis

This document provides a detailed analysis of the **Gemini Service** implementation, the **Recommendation Service** process flows, and the **Specialist Review Automation** flow, detailing the behaviors in both success and error scenarios.

---

## 1. Gemini Service (`GeminiService.cs`)

The [GeminiService](file:///c:/Users/Zoon/source/repos/Horr/ServiceImplementation/Implementations/AI/GeminiService.cs) is a wrapper around the Google Gemini API (specifically `gemini-2.0-flash`), designed to provide highly predictable JSON responses.

### Request Configuration
- **Model**: `gemini-2.0-flash`
- **Temperature**: `0.1` (configured for high predictability/determinism).
- **Max Output Tokens**: `300` (optimized for short responses such as lists of IDs or simple JSON objects).
- **Timeout**: `8` seconds (implemented via `client.Timeout = TimeSpan.FromSeconds(8)` to fail fast).
- **Response Format**: Configured as `responseMimeType = "application/json"` with a schema constraining the output to an `ARRAY` of `STRING`s.

### Success Case
1. **API Key Verification**: The service retrieves the API key from `config["Gemini:ApiKey"]`.
2. **Payload Construction**: Prepares the prompt and sets parameters (temperature, timeout, MIME types).
3. **HTTP Call**: Sends a `POST` request to:
   `https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}`
4. **Parsing**: If the HTTP response status code is in the 2xx range, it reads the response content, parses it as a `JsonDocument`, and extracts the generated text using:
   `RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString()`
5. **Output**: Returns the trimmed JSON text string (e.g., `["jobId1", "jobId2"]`).

### Error Case
- **Configuration Error**: If `Gemini:ApiKey` is missing or null, the constructor throws an exception: `"Gemini API key not configured."`
- **Timeout Error**: If the Gemini API does not respond within `8` seconds, a `TaskCanceledException` is thrown by `PostAsync`.
- **API Server Error**: If the API returns a non-2xx status code, the service reads the response body and throws:
  `new Exception($"Gemini API error: {error}")`
- **Schema/Parsing Error**: If the response is not valid JSON or is missing the expected properties (like `candidates` or `parts`), `JsonDocument.Parse` or `GetProperty` throws a standard JSON/key exception which propagates to the caller.

---

## 2. Recommendation Process (`RecommendationService.cs`)

The [RecommendationService](file:///c:/Users/Zoon/source/repos/Horr/ServiceImplementation/Implementations/Recommendations/RecommendationService.cs) contains two primary AI-driven recommendation paths, both utilizing a **Two-Stage Retrieval** strategy (SQL pre-filtering followed by LLM reranking/selection).

### A. Freelancer Job Recommendations (`GetRecommendedJobsForFreelancerAsync`)

#### Workflow Steps
1. **Profile Loading**: Fetches the freelancer profile, skills, bio, hourly rate, and experience level. If the freelancer is not found, returns an empty list.
2. **Exclusion**: Fetches IDs of jobs the freelancer has already applied to (`appliedJobIds`).
3. **Behavior History**: Fetches the 10 most recent job interactions (views, saves, etc.).
4. **SQL Retrieval (Stage 1)**: Queries the database for up to 40 active, unapplied jobs with at least one overlapping skill, ordered by post date.
5. **SQL Fallback (Stage 2)**: If no jobs overlap in skills, queries the 40 most recent active, unapplied jobs and sets `isFallback = true`.
6. **Gemini Invocation**: Builds a detailed prompt (freelancer info, interactions, candidate jobs) asking Gemini to select the top 5 job IDs.
7. **Mapping**: Fetches full details for the returned job IDs, orders them by Gemini's preference index, and returns `RecommendedJobDTO`s.

#### Success and Error Cases

| Case | Scenario Details | Result / Behavior |
| :--- | :--- | :--- |
| **Success** | Database queries succeed, Gemini responds under 8 seconds with a valid JSON array of job IDs, and the database contains these jobs. | Returns a list of up to 5 `RecommendedJobDTO`s ranked by Gemini, matching the freelancer's skills/preferences. |
| **Error (Gemini Down/Timeout)** | `_gemini.AskAsync(prompt)` fails (timeout, API error, or network loss). | **Graceful Fallback**: Enters the `catch` block, takes the top 5 candidate jobs directly from the SQL retrieval step, and returns them as `RecommendedJobDTO`s (with `IsFallback = true`, `IsSaved = false`). |
| **Error (JSON Deserialization Failure)** | Gemini returns text that cannot be parsed as a JSON array of strings. | **Graceful Fallback**: Enters the `catch` block and returns the top 5 candidate jobs from SQL pre-filtering. |
| **Edge Case (No Candidate Jobs)** | No jobs exist in the DB (or all have been applied to). | Returns an empty `List<RecommendedJobDTO>` immediately without calling Gemini. |

---

### B. Client Freelancer Recommendations (`GetRecommendedFreelancersForClientAsync`)

#### Workflow Steps
1. **Context Collection**: Gets the client's 5 most recent active job posts and distinct skills needed.
2. **Exclusion**: Excludes freelancers whom the client has already hired.
3. **Behavior History**: Gets the client's 10 most recent interactions with freelancers.
4. **SQL Retrieval (Stage 1)**: Queries up to 40 freelancers possessing skills needed for the client's jobs.
5. **SQL Fallback (Stage 2)**: If no overlapping freelancers are found, queries up to 40 freelancers overall.
6. **Gemini Invocation**: Sends client context, interactions, and candidate freelancers to Gemini, requesting up to 5 freelancer IDs.
7. **Mapping**: Fetches database records for the chosen IDs and returns `RecommendedFreelancerDTO`s.

#### Success and Error Cases

| Case | Scenario Details | Result / Behavior |
| :--- | :--- | :--- |
| **Success** | Gemini successfully returns a JSON array of freelancer IDs. | Returns a list of up to 5 `RecommendedFreelancerDTO`s, ordered by Gemini's relevance recommendation. |
| **Error (Gemini Call / Parse Failure)** | API timeout, failure response, or malformed JSON output from Gemini. | **Graceful Fallback**: Catches the exception, takes the first 5 candidate freelancers from the pre-filtering database query, and maps them to DTOs. |
| **Edge Case (No Candidates)** | No freelancers exist in the system (excluding already hired ones). | Returns an empty `List<RecommendedFreelancerDTO>` immediately without calling Gemini. |

---

## 3. Specialist Review Automation (`RequestSpecialistReviewCommandHandler.cs`)

Gemini is also used in [RequestSpecialistReviewCommandHandler](file:///c:/Users/Zoon/source/repos/Horr/ServiceImplementation/Implementations/Contracts/RequestSpecialistReviewCommandHandler.cs) to automatically review freelancer deliverables against client requirements.

### Workflow Steps
1. Builds a prompt detailing client requirements, delivery note, and readable file attachments.
2. Prompts Gemini to return a verdict of `"Satisfactory"` or `"Unsatisfactory"` along with a brief `note` explaining its reasoning.
3. Requests either a JSON object `{"verdict": "...", "note": "..."}` or a JSON array `["verdict", "note"]`.

### Success and Error Cases

| Case | Scenario Details | Result / Behavior |
| :--- | :--- | :--- |
| **Success (Perfect JSON Object)** | Gemini returns a valid JSON object. | Extracts `verdict` and `note` fields. Sets the specialist review record to `Completed` with the correct verdict and note. |
| **Success (JSON Array Format)** | Gemini returns a JSON array of strings instead. | Extracts element `[0]` as verdict and `[1]` as note. Sets the review record to `Completed`. |
| **Error (Gemini Fail/Timeout)** | `AskAsync` throws an exception. | `responseText` remains empty. The review is marked `Completed`, but defaults to: <br>• **Verdict**: `Unsatisfactory`<br>• **Note**: `"AI review failed to produce a valid response. Please request a human specialist review."` |
| **Error (Malformed Response / Parse Failure)** | Gemini returns non-JSON or a JSON missing the expected fields. | Catches the parsing exception and preserves the default values: <br>• **Verdict**: `Unsatisfactory`<br>• **Note**: `"AI review failed to produce a valid response. Please request a human specialist review."` |

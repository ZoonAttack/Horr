using System;
using Entities.Users;
using Services.DTOs.UserDTOs.Client;

namespace Services.DTOs.UserDTOs.Client
{
    public static class ClientMapper
    {
        public static ClientReadDTO Client_To_ClientRead(this Entities.Users.User user)
        {
            if (user == null)
            {
                return null;
            }

            return new ClientReadDTO
            {
                // User Mapping
                Id = user.Id,
                Role = user.Role,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.PhoneNumber,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
            };
        }

        public static Entities.Users.User ClientCreate_To_User(this ClientCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.Users.User
            {
                FullName = createDto.FullName,
                Email = createDto.Email,
                PhoneNumber = createDto.Phone
            };
        }

        public static void ClientUpdate_To_Client(this Entities.Users.User user, ClientUpdateDTO updateDto)
        {
            if (user == null || updateDto == null)
            {
                return;
            }

            // Apply updates to the User entity
            user.FullName = updateDto.FullName;
            user.Email = updateDto.Email;
            user.PhoneNumber = updateDto.Phone;

        }
    }
}
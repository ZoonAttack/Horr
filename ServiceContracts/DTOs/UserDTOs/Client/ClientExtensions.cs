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
                TrustScore = user.TrustScore,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                // Client Profile Mapping
                Bio = user.Client?.Bio
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
                PhoneNumber = createDto.Phone,

                // Initialize the Client navigation property with the specific data
                Client = new Entities.Users.Client
                {
                    Bio = createDto.Bio
                }
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

            // Apply updates to the Client Profile
            if (user.Client != null)
            {
                user.Client.Bio = updateDto.Bio;
            }
        }
    }
}
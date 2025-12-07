using System;
using Entities.User;
using ServiceContracts.DTOs.User.Client;

namespace ServiceContracts.DTOs.User.Client
{
    public static class ClientMapper
    {
        public static ClientReadDTO Client_To_ClientRead(this Entities.User.User user)
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
                Phone = user.Phone,
                IsVerified = user.IsVerified,
                TrustScore = user.TrustScore,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,

                // Client Profile Mapping
                Bio = user.Client?.Bio
            };
        }

        public static Entities.User.User ClientCreate_To_User(this ClientCreateDTO createDto)
        {
            if (createDto == null)
            {
                return null;
            }

            return new Entities.User.User
            {
                FullName = createDto.FullName,
                Email = createDto.Email,
                Phone = createDto.Phone,

                // Initialize the Client navigation property with the specific data
                Client = new Entities.User.Client
                {
                    Bio = createDto.Bio
                }
            };
        }

        public static void ClientUpdate_To_Client(this Entities.User.User user, ClientUpdateDTO updateDto)
        {
            if (user == null || updateDto == null)
            {
                return;
            }

            // Apply updates to the User entity
            user.FullName = updateDto.FullName;
            user.Email = updateDto.Email;
            user.Phone = updateDto.Phone;

            // Apply updates to the Client Profile
            if (user.Client != null)
            {
                user.Client.Bio = updateDto.Bio;
            }
        }
    }
}
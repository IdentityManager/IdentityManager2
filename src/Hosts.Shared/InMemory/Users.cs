using System;
using System.Collections.Generic;
using System.Security.Claims;
using IdentityManager2;
using Microsoft.AspNetCore.Mvc;

namespace Hosts.Shared.InMemory
{
    public class Users
    {
        public static ICollection<InMemoryUser> Get(int random = 0)
        {
            var users = new HashSet<InMemoryUser>
            {
                new InMemoryUser{
                    Subject = Guid.Parse("081d965f-1f84-4360-90e4-8f6deac7b9bc").ToString(),
                    Username = "alice",
                    Password = "alice",
                    Email = "alice@email.com",
                    Mobile = "123",
                    Claims = new HashSet<Claim>{
                        new Claim(IdentityManagerConstants.ClaimTypes.Name, "Alice Smith"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "admin"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "employee"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "manager"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, IdentityManagerConstants.AdminRoleName),
                        new Claim("department", "sales"),
                    }
                },
                new InMemoryUser{
                    Subject = Guid.Parse("5f292677-d3d2-4bf9-a6f8-e982d08e1306").ToString(),
                    Username = "bob",
                    Password = "bob",
                    Email = "bob@email.com",
                    Claims = new HashSet<Claim>{
                        new Claim(IdentityManagerConstants.ClaimTypes.Name, "Bob Smith"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "employee"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "developer"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, IdentityManagerConstants.AdminRoleName),
                        new Claim("department", "IT"),
                    }
                },
                new InMemoryUser{
                    Subject = Guid.NewGuid().ToString(),
                    Username = "test",
                    Password = "test",
                    Email = "test@email.com",
                    Claims = new HashSet<Claim>{
                        new Claim(IdentityManagerConstants.ClaimTypes.Name, "Test"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "employee"),
                        new Claim(IdentityManagerConstants.ClaimTypes.Role, "developer"),
                        new Claim("department", "IT"),
                    }
                },
            };

            for (var i = 0; i < random; i++)
            {
                var user = new InMemoryUser
                {
                    Username = GenName().ToLower(),
                    Password = GenName().ToLower()
                };
                user.Claims.Add(new Claim("name", GenName() + " " + GenName()));
                users.Add(user);
            }

            return users;
        }

        private static string GenName()
        {
            var firstChar = (char)(Random.Next(26) + 65);
            var username = firstChar.ToString();
            for (var j = 0; j < 6; j++)
            {
                username += Char.ToLower((char)(Random.Next(26) + 65));
            }
            return username;
        }

        private static readonly Random Random = new Random();
    }
}

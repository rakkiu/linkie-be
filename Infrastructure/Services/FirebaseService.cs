using System;
using System.Threading.Tasks;
using Application.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseAuth _auth;

        public FirebaseService(IConfiguration config)
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                // Option 1: Read full JSON from Environment Variable (Best for Cloud Deploy: Render, Docker, etc.)
                var jsonConfig = config["Firebase:ConfigJson"] ?? Environment.GetEnvironmentVariable("FIREBASE_CONFIG_JSON");
                
                if (!string.IsNullOrEmpty(jsonConfig))
                {
                    Console.WriteLine(">>> FirebaseService: Initializing using FIREBASE_CONFIG_JSON environment variable.");
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromJson(jsonConfig)
                    });
                }
                else
                {
                    // Option 2: Read from File (Local development)
                    var serviceAccountFile = config["Firebase:ServiceAccountPath"] 
                                             ?? "hexa-linkie-firebase-adminsdk-fbsvc-27855a727e.json";
                    
                    string[] searchPaths = {
                        Path.GetFullPath(serviceAccountFile),
                        Path.Combine(AppContext.BaseDirectory, serviceAccountFile),
                        Path.Combine(Directory.GetCurrentDirectory(), serviceAccountFile),
                        Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory())?.FullName ?? "", serviceAccountFile)
                    };

                    string? finalPath = null;
                    foreach (var path in searchPaths)
                    {
                        if (File.Exists(path))
                        {
                            finalPath = path;
                            break;
                        }
                    }

                    if (finalPath == null)
                    {
                        Console.WriteLine($">>> FirebaseService ERROR: No FIREBASE_CONFIG_JSON found and service account file NOT FOUND.");
                        throw new FileNotFoundException($"Firebase configuration not found.");
                    }

                    Console.WriteLine($">>> FirebaseService: Using service account file: {finalPath}");
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(finalPath)
                    });
                }
            }
            _auth = FirebaseAuth.DefaultInstance;
        }

        public async Task<FirebaseUserInfo?> VerifyIdTokenAsync(string idToken)
        {
            try
            {
                Console.WriteLine(">>> FirebaseService: Verifying ID Token...");
                var decodedToken = await _auth.VerifyIdTokenAsync(idToken);
                Console.WriteLine($">>> FirebaseService: Decoded token for UID: {decodedToken.Uid}");
                
                // Fetch user record to get name/picture if not in token
                var userRecord = await _auth.GetUserAsync(decodedToken.Uid);
                Console.WriteLine($">>> FirebaseService: Fetched user record for: {userRecord.Email}");

                return new FirebaseUserInfo
                {
                    FirebaseUid = decodedToken.Uid,
                    Email = userRecord.Email,
                    Name = userRecord.DisplayName ?? userRecord.Email.Split('@')[0],
                    Picture = userRecord.PhotoUrl
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($">>> FirebaseService ERROR: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }
    }
}

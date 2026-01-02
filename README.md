# HevyHeart - Strava Heart Rate Sync for Hevy

HevyHeart is a console application that synchronizes heart rate data from Strava activities to Hevy workouts. This allows you to track your cardiovascular performance during strength training sessions by combining Strava's heart rate monitoring with Hevy's workout tracking.

## Overview

The application:
- Authenticates with both Strava and Hevy APIs
- Fetches your recent Strava activities with heart rate data
- Retrieves your Hevy workouts
- Synchronizes heart rate data from a selected Strava activity to a selected Hevy workout
- Creates a new Hevy workout with the heart rate data included

## Prerequisites

Before using HevyHeart, you need:

1. **Strava API Credentials**
   - Client ID
   - Client Secret

2. **Hevy API Key**
   - API Key from Hevy

3. **Hevy Account Credentials**
   - Username or Email
   - Password

4. **.NET 9 Runtime or SDK**
   - Required to run the application

## Getting API Credentials

### Strava API Setup

1. Go to [Strava API Settings](https://www.strava.com/settings/api)
2. Log in to your Strava account
3. Create a new application by clicking **Create An App** (or **My API Application** if you already have one)
4. Fill in the required information:
   - **Application Name**: HevyHeart (or any name you prefer)
   - **Category**: Choose the most appropriate category (e.g., "Training & Planning")
   - **Club**: Optional - leave blank if not applicable
   - **Website**: Can be your personal website, GitHub repository, or just `http://localhost`
   - **Application Description**: Brief description of what the app does
   - **Authorization Callback Domain**: Enter `localhost` (this is critical!)
5. Agree to the API terms and create the application
6. After creation, you'll see your application page with:
   - **Client ID**: A numeric value - copy this
   - **Client Secret**: An alphanumeric string - copy this (keep it secure!)
   - You can click "show" next to Client Secret to reveal it

**Important:** Keep your Client Secret secure and never commit it to public repositories!

### Hevy API Key

1. Visit the [Hevy Developer Portal](https://hevyapp.com) or check their documentation
2. Sign in to your Hevy account
3. Navigate to API settings or developer settings
4. Request or generate an API key
5. Copy and save your API key securely

**Note:** If you're unable to find API settings, you may need to contact Hevy support to request API access.

### Hevy Account Password

**Important Note for Third-Party Authentication Users:**

If you currently sign in to Hevy using **Google**, **Apple**, or another third-party authentication provider, you **can still use this application**! Hevy allows you to set a password for your account even if you typically use social login. This password is specifically for API authentication and doesn't affect your ability to continue using social login for normal access.

#### How to Set a Password for Social Login Users:

1. Open the **Hevy mobile app** or visit the **Hevy website**
2. Go to **Settings** → **Account** → **Security** (or similar path)
3. Look for an option to **Set Password**, **Add Password**, or **Change Password**
4. Create a strong password for your Hevy account
5. Save your changes
6. Use this password in the HevyHeart application configuration
7. **You can continue to use Google/Apple sign-in** for your normal Hevy app usage

If you cannot find the password setting option:
- Check if there's an "Account Security" or "Login Methods" section
- Try accessing Hevy from a web browser instead of the mobile app
- Contact Hevy support for assistance in setting up a password for API access

## Configuration

### Setting up appsettings.json

The application requires configuration through the `appsettings.json` file located in the `HevyHeartConsole` directory.

1. Navigate to the `HevyHeartConsole` folder in your cloned repository
2. Open `appsettings.json` in a text editor (Visual Studio, VS Code, Notepad++, etc.)
3. Update the configuration with your credentials:

```json
{
  "Strava": {
    "ClientId": "YOUR_STRAVA_CLIENT_ID",
    "ClientSecret": "YOUR_STRAVA_CLIENT_SECRET",
    "RedirectUri": "http://localhost:8080/callback",
    "Scope": "read,activity:read_all"
  },
  "Hevy": {
    "ApiKey": "YOUR_HEVY_API_KEY",
    "BaseUrl": "https://api.hevyapp.com",
    "AuthToken": null,
    "EmailOrUsername": "YOUR_HEVY_EMAIL_OR_USERNAME",
    "Password": "YOUR_HEVY_PASSWORD"
  },
  "Server": {
    "Port": 8080,
    "Host": "localhost"
  }
}
```

### Configuration Fields Explained

#### Strava Section

| Field | Description | Required | Default Value |
|-------|-------------|----------|---------------|
| `ClientId` | Your Strava API Client ID (numeric) | ✅ Yes | - |
| `ClientSecret` | Your Strava API Client Secret (alphanumeric string) | ✅ Yes | - |
| `RedirectUri` | OAuth callback URL | ✅ Yes | `http://localhost:8080/callback` |
| `Scope` | API permissions requested | ✅ Yes | `read,activity:read_all` |

**⚠️ Important:** 
- Do not change `RedirectUri` unless you also update your Strava API application settings
- Do not modify `Scope` unless you understand OAuth permissions
- The port in `RedirectUri` must match the `Server.Port` setting

#### Hevy Section

| Field | Description | Required | Default Value |
|-------|-------------|----------|---------------|
| `ApiKey` | Your Hevy API Key | ✅ Yes | - |
| `BaseUrl` | Hevy API endpoint | ✅ Yes | `https://api.hevyapp.com` |
| `AuthToken` | Optional V2 API token for advanced features | ❌ Optional | `null` |
| `EmailOrUsername` | Your Hevy account email or username | ✅ Yes* | - |
| `Password` | Your Hevy account password | ✅ Yes* | - |

*Required unless you have a pre-configured `AuthToken`. Most users should leave `AuthToken` as `null` and provide credentials.

**Security Notes:** 
- Your credentials are stored locally and only transmitted to official Strava/Hevy APIs over HTTPS
- Never commit your `appsettings.json` with real credentials to a public repository
- Consider using environment variables or secret management for production deployments

#### Server Section

| Field | Description | Required | Default Value |
|-------|-------------|----------|---------------|
| `Port` | Local server port for OAuth callback | ✅ Yes | `8080` |
| `Host` | Local server host | ✅ Yes | `localhost` |

**When to change these:**
- If port 8080 is already in use by another application
- If you modify these, update `Strava.RedirectUri` accordingly

## Running the Application

### Using Visual Studio

1. Open the `HevyHeart.sln` solution file in Visual Studio
2. Ensure `HevyHeartConsole` is set as the startup project
   - Right-click on `HevyHeartConsole` in Solution Explorer
   - Select **Set as Startup Project**
3. Press **F5** to run with debugging, or **Ctrl+F5** to run without debugging
4. The console window will open and guide you through the process

### Using Command Line (.NET CLI)

1. Open a terminal/command prompt/PowerShell window
2. Navigate to the `HevyHeartConsole` directory:
   ```bash
   cd HevyHeartConsole
   ```
3. Build the application (first time only):
   ```bash
   dotnet build
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

### Using Published Executable

To create a standalone executable:

1. Open a terminal in the solution root directory
2. Run the publish command:
   ```bash
   # For Windows 64-bit
   dotnet publish -c Release -r win-x64 --self-contained
   
   # For Windows 64-bit (single file)
   dotnet publish -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
   
   # For macOS (Intel)
   dotnet publish -c Release -r osx-x64 --self-contained
   
   # For macOS (Apple Silicon)
   dotnet publish -c Release -r osx-arm64 --self-contained
   
   # For Linux
   dotnet publish -c Release -r linux-x64 --self-contained
   ```
3. Navigate to the publish directory (e.g., `HevyHeartConsole\bin\Release\net9.0\win-x64\publish\`)
4. Copy the entire contents to your desired location
5. Run the executable:
   - Windows: Double-click `HevyHeartConsole.exe` or run from command line
   - macOS/Linux: `./HevyHeartConsole` from terminal

## Application Workflow

When you run HevyHeart, it guides you through an interactive workflow:

### Step 0: Hevy Authentication
```
--- Step 0: Hevy Authentication ---
```
- The app authenticates with Hevy using your configured credentials
- If credentials are provided in `appsettings.json`, authentication is automatic
- If not configured, you'll be prompted to enter your email/username and password manually
- Your password input is hidden for security

**Possible outcomes:**
- ✅ Success: "Successfully authenticated with Hevy"
- ❌ Error: "Failed to authenticate with Hevy" - check credentials

### Step 1: Strava Authentication
```
--- Step 1: Strava Authentication ---
```
- A web browser window will automatically open to Strava's authorization page
- Log in to Strava (if not already logged in)
- Review the requested permissions
- Click **Authorize** to grant the application access
- The browser will redirect to a success page
- Return to the console - authentication will complete automatically

**Manual steps (if browser doesn't open):**
- Copy the URL displayed in the console
- Paste it into your web browser
- Complete the authorization
- The app will detect the callback automatically

### Step 2: Select Strava Activity
```
--- Step 2: Fetching Strava Activities ---
```
- The app fetches your recent Strava activities that have heart rate data
- A numbered list is displayed showing:
  - Activity name
  - Date and time
  - Activity type (Run, Ride, Workout, etc.)
  - Duration
  - Average heart rate (if available)

**Example:**
```
Found 10 activities with heart rate data:

 1. Morning Run
     Date: 2026-01-01 07:30
     Type: Run
     Duration: 00:45:23
     Avg HR: 142 bpm

 2. Evening Strength Training
     Date: 2025-12-31 18:00
     Type: Workout
     Duration: 01:15:00
     Avg HR: 128 bpm

Select an activity (1-10):
```

- Enter the number corresponding to your desired activity
- The activity will be selected and detailed data fetched

### Step 3: Fetch Detailed Strava Data
```
--- Step 3: Fetching Strava Activity Details ---
```
- The app retrieves:
  - Complete activity details
  - Heart rate stream data (second-by-second measurements)
- Data is automatically saved to local JSON files for debugging:
  - `strava_activity_{id}_{timestamp}.json`
  - `strava_heartrate_{id}_{timestamp}.json`

### Step 4: Select Hevy Workout
```
--- Step 4: Fetching Hevy Workouts ---
```
- The app fetches your recent Hevy workouts
- A numbered list is displayed showing:
  - Workout title
  - Date and time
  - Duration
  - Number of exercises
  - Description (if available)

**Example:**
```
Found 15 Hevy workouts:

 1. Upper Body Power
     Date: 2026-01-01 18:00
     Duration: 01:15:30
     Exercises: 8
     Description: Chest and back focus

 2. Leg Day
     Date: 2025-12-30 17:30
     Duration: 01:30:00
     Exercises: 6

Select a workout (1-15):
```

- Enter the number corresponding to your desired workout
- The workout data is fetched and saved to:
  - `hevy_workout_fromlist_{id}_{timestamp}.json`

### Step 5: Synchronize Heart Rate Data
```
--- Step 5: Synchronizing Heart Rate Data ---
```
- The app processes and synchronizes the heart rate data
- Shows a summary:
  ```
  Synchronizing heart rate data from Strava activity 'Evening Strength Training'
  to Hevy workout 'Upper Body Power'
  
  Generated 4530 heart rate samples
  Total calories: 629
  Heart rate - Min: 85 bpm, Avg: 128 bpm, Max: 167 bpm
  ```
- Saves synchronized data to:
  - `synchronized_biometrics_{workout_id}_{timestamp}.json`
- **Confirmation prompt:**
  ```
  Do you want to update the Hevy workout with this heart rate data? (y/N):
  ```
  - Enter `y` or `yes` to proceed
  - Enter `n` or `no` (or just press Enter) to skip

### Step 6: Update Hevy Workout
If you confirmed the update:
- The app creates a **new** Hevy workout with heart rate data
- Shows success message with important information

**⚠️ CRITICAL INFORMATION:**
```
✅ Hevy workout updated successfully!

⚠️  IMPORTANT: A new workout with heart rate data has been created in Hevy.
   Old workout ID: abc123
   
📱 Please check the Hevy app to verify the new workout appears correctly
   before deleting the old one to avoid duplicates.
   
Have you verified the new workout in Hevy and want to delete the old one? (y/N):
```

**Why a new workout is created:**
- The Hevy API creates a new workout rather than updating the existing one
- This preserves your original data until you verify the sync was successful
- Prevents data loss if something goes wrong

**Recommended workflow:**
1. Let the app create the new workout
2. Open the Hevy mobile app on your phone
3. Find and review the new workout (it will have the same name, date, and exercises)
4. Verify the heart rate data appears correctly
5. Return to the console and confirm deletion (`y`)
6. The old workout will be automatically deleted

**If you choose not to delete immediately:**
- The old workout remains in your Hevy account
- You'll see duplicate workouts for that session
- You can manually delete the old workout later from the Hevy app

### Completion
```
✅ Heart rate synchronization completed successfully!
Press any key to exit...
```

The application has successfully:
- ✅ Authenticated with Strava and Hevy
- ✅ Retrieved heart rate data from Strava
- ✅ Synchronized data to match your Hevy workout timeline
- ✅ Created a new Hevy workout with heart rate data
- ✅ (Optionally) Deleted the old workout to prevent duplicates

## How Heart Rate Synchronization Works

### Technical Details

1. **Duration Matching**: 
   - Strava heart rate data is trimmed to exactly match the Hevy workout duration
   - Start and end times are aligned precisely

2. **Timestamp Alignment**: 
   - Heart rate samples are created at 1-second intervals
   - Each sample uses Unix timestamps (milliseconds)
   - Timestamps align with the Hevy workout's start time

3. **Data Interpolation**: 
   - If Strava samples are sparse, data is interpolated to create smooth transitions
   - If Strava samples are dense, data is aggregated to 1-second intervals
   - Ensures consistent data quality regardless of recording device

4. **Calorie Transfer**: 
   - Total calories from Strava activity are included
   - Hevy uses this for workout metrics

### Data Structure

The synchronized biometrics follow Hevy's JSON structure:

```json
{
  "biometrics": {
    "total_calories": 629.0,
    "heart_rate_samples": [
      {
        "timestamp_ms": 1735682400000,
        "bpm": 97
      },
      {
        "timestamp_ms": 1735682401000,
        "bpm": 98
      }
      // ... more samples at 1-second intervals
    ]
  }
}
```

## Data Files Created

The application saves several JSON files during execution for debugging and verification:

| File Pattern | Description | Purpose |
|--------------|-------------|---------|
| `strava_activity_{id}_{timestamp}.json` | Complete Strava activity details | Debugging, backup |
| `strava_heartrate_{id}_{timestamp}.json` | Raw heart rate stream from Strava | Debugging, verification |
| `hevy_workout_fromlist_{id}_{timestamp}.json` | Hevy workout data | Debugging, verification |
| `synchronized_biometrics_{id}_{timestamp}.json` | Final synchronized heart rate data | Verification, manual import |

**Note:** These files are created in the same directory where you run the application.

**Cleanup:** You can safely delete these files after successful synchronization, or keep them for troubleshooting future issues.

## Troubleshooting

### Configuration Issues

#### "❌ Strava Client ID not configured"
- Open `appsettings.json`
- Replace `"YOUR_STRAVA_CLIENT_ID"` with your actual Client ID
- Ensure no quotation marks are missing
- Save the file

#### "❌ Hevy API Key not configured"
- Verify you've obtained an API key from Hevy
- Replace `"YOUR_HEVY_API_KEY"` in `appsettings.json`
- Ensure the key is correct (no extra spaces or characters)

#### "⚠️ Hevy V2 API tokens or credentials not configured"
- This is a warning, not an error
- Add your Hevy `EmailOrUsername` and `Password` to `appsettings.json`
- Alternatively, you can log in manually when prompted

### Authentication Issues

#### "Failed to authenticate with Hevy"
**Possible causes:**
- Incorrect username/email or password
- API key is invalid or expired
- Network connectivity issues

**Solutions:**
1. Double-check credentials in `appsettings.json`
2. If using third-party login (Google/Apple), ensure you've created a password for API access
3. Try logging in to Hevy's website/app to verify credentials work
4. Contact Hevy support to verify API key is active

#### Strava Authorization Browser Doesn't Open
**Solution:**
1. Copy the URL displayed in the console
2. Manually paste it into your web browser
3. Complete the authorization
4. The app will automatically detect the callback

#### "Failed to exchange authorization code for access token"
**Possible causes:**
- Callback redirect failed
- Port 8080 is blocked or in use
- `RedirectUri` doesn't match Strava API settings

**Solutions:**
1. Check your Strava API application settings
2. Ensure "Authorization Callback Domain" is set to `localhost`
3. Try changing the port in `appsettings.json` (both `Server.Port` and `Strava.RedirectUri`)
4. Temporarily disable firewall/antivirus

### Data Issues

#### "❌ No activities with heart rate data found"
**Possible causes:**
- Your recent Strava activities don't have heart rate data
- Heart rate monitor wasn't connected during activities
- Authorization scope is insufficient

**Solutions:**
1. Ensure your Strava activities were recorded with a heart rate monitor
2. Check that activities have uploaded successfully to Strava
3. Record a new activity with heart rate data
4. Verify `Scope` in `appsettings.json` includes `activity:read_all`

#### "❌ No Hevy workouts found"
**Possible causes:**
- No workouts in your Hevy account
- API key doesn't have permission to read workouts
- Date range is outside available workouts

**Solutions:**
1. Log a workout in the Hevy app first
2. Verify API key permissions
3. Check network connectivity

#### "❌ Failed to get detailed activity data or heart rate stream"
**Possible causes:**
- Network issues
- Strava API rate limiting
- Activity doesn't have heart rate data

**Solutions:**
1. Check internet connection
2. Wait a few minutes and try again (rate limiting)
3. Select a different activity

### Update Issues

#### "❌ Failed to update Hevy workout"
**Possible causes:**
- API key lacks write permissions
- Hevy V2 API endpoint is not available/accessible
- Network issues

**Solutions:**
1. Check the generated `synchronized_biometrics_*.json` file
2. You can attempt to manually import this data to Hevy
3. Verify your API key has write permissions
4. Contact Hevy support about V2 API access

#### "⚠️ Warning: Failed to delete old Hevy workout"
**Not critical** - This means:
- The new workout was created successfully
- The old workout wasn't deleted automatically
- You need to manually delete the old workout in the Hevy app

**Solution:**
- Open Hevy app
- Find the duplicate workout (same name, date, but no heart rate data)
- Swipe to delete or use the menu to remove it

### Network and Port Issues

#### "Port 8080 already in use" or callback server fails
**Solution:**
1. Close other applications using port 8080
2. Or change the port:
   - Edit `appsettings.json`
   - Change `Server.Port` to a different port (e.g., `8081`, `8888`, `3000`)
   - Update `Strava.RedirectUri` to match: `http://localhost:8081/callback`
3. Restart the application

#### Firewall/Antivirus Blocking
**Solution:**
1. Temporarily disable firewall/antivirus
2. If that works, add an exception for:
   - `HevyHeartConsole.exe`
   - Port 8080 (or your configured port)
3. Re-enable firewall/antivirus

### General Issues

#### Application Crashes or Freezes
**Solutions:**
1. Check the console for error messages
2. Review the last operation that was running
3. Ensure you have .NET 9 runtime installed
4. Try running from command line to see full error output:
   ```bash
   dotnet run --project HevyHeartConsole
   ```

#### Data Looks Incorrect
**Solutions:**
1. Review the generated JSON files
2. Verify the Strava activity and Hevy workout are the correct ones
3. Check that times/dates align properly
4. Contact support with the JSON files for investigation

## API Endpoints Used

### Strava API v3

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/oauth/authorize` | GET | Initial OAuth authorization |
| `/oauth/token` | POST | Exchange authorization code for access token |
| `/athlete/activities` | GET | Fetch user's activities |
| `/activities/{id}` | GET | Get detailed activity information |
| `/activities/{id}/streams` | GET | Get heart rate stream data |

**Documentation:** [Strava API Documentation](https://developers.strava.com/docs/reference/)

### Hevy API

| Endpoint | Method | Purpose |
|----------|--------|---------|
| `/v1/workouts` | GET | Fetch user's workouts |
| `/v2/workouts/{id}` | GET | Get detailed workout information |
| `/v2/workouts` | POST | Create new workout with biometrics |
| `/v2/workouts/{id}` | DELETE | Delete a workout |

**Note:** Hevy V2 API endpoints are undocumented and may change. The application attempts to use these for biometrics updates.

## Privacy and Security

### Data Handling
- ✅ All configuration data is stored locally on your machine
- ✅ Credentials are only transmitted to official Strava and Hevy APIs over HTTPS
- ✅ No third-party services receive your data
- ✅ No telemetry or analytics are collected
- ✅ API tokens are stored in memory and cleared when the application closes

### Best Practices
- 🔒 Never commit `appsettings.json` with real credentials to public repositories
- 🔒 Use `.gitignore` to exclude sensitive files
- 🔒 Rotate your API keys periodically
- 🔒 Only grant necessary OAuth permissions
- 🔒 Keep your Client Secret secure

### Open Source
- 📖 The source code is open and available for review
- 🔍 You can audit exactly what the application does
- 🤝 Community contributions are welcome

## System Requirements

- **Operating System:** Windows 10/11, macOS 10.15+, or Linux
- **.NET:** .NET 9 Runtime or SDK
- **Memory:** Minimum 512MB RAM
- **Disk Space:** ~50MB for application + space for JSON data files
- **Internet:** Required for API communication
- **Browser:** Any modern web browser for Strava OAuth

## Known Limitations

- ⚠️ Hevy V2 API is undocumented and may change without notice
- ⚠️ Creating a new workout instead of updating existing (by design)
- ⚠️ No batch processing - processes one activity/workout at a time
- ⚠️ Limited error recovery - most errors require restart
- ⚠️ No GUI - console-only interface

## Frequently Asked Questions

### Q: Do I need to keep the console window open during synchronization?
**A:** Yes, keep it open until you see the "Press any key to exit..." message.

### Q: Can I sync multiple activities at once?
**A:** No, currently the application processes one activity-to-workout pair at a time. Run the application multiple times for multiple syncs.

### Q: Will this work with Apple Watch/Garmin/Polar heart rate data?
**A:** Yes! As long as the heart rate data is recorded in your Strava activity, it doesn't matter what device captured it.

### Q: What happens if my Strava activity is longer than my Hevy workout?
**A:** The heart rate data is automatically trimmed to match the exact duration of your Hevy workout.

### Q: What happens if my Hevy workout is longer than my Strava activity?
**A:** You should select a Strava activity that covers the full duration of your workout for best results. The application won't extrapolate missing data.

### Q: Can I edit the heart rate data before syncing?
**A:** Not through the application UI, but you can manually edit the `synchronized_biometrics_*.json` file before importing it to Hevy.

### Q: Is this application officially supported by Strava or Hevy?
**A:** No, this is an independent third-party application using public APIs. It is not officially affiliated with or endorsed by Strava or Hevy.

### Q: My data didn't sync correctly. Can I undo it?
**A:** The old workout is preserved initially. If you haven't deleted it yet, just delete the new workout and keep the old one. If you already deleted the old workout, you'll need to recreate it manually in Hevy.

### Q: Does this cost money to use?
**A:** The application itself is free and open-source. However, you need:
- Strava account (free or paid)
- Hevy account (free or paid)
- API access (check with Hevy for any associated costs)

## Support and Contributing

### Getting Help
- 🐛 **Bug Reports:** [GitHub Issues](https://github.com/iAm9001/HevyHeart/issues)
- 💡 **Feature Requests:** [GitHub Issues](https://github.com/iAm9001/HevyHeart/issues)
- 📖 **Documentation:** This README and code comments
- 💬 **Questions:** [GitHub Discussions](https://github.com/iAm9001/HevyHeart/discussions)

### Contributing
Contributions are welcome! Here's how:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Test thoroughly
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

**Areas where contributions are especially welcome:**
- Improved error handling and recovery
- GUI/Web interface
- Batch processing capabilities
- Additional API integrations
- Better documentation
- Unit tests

## Changelog

See [CHANGELOG.md](CHANGELOG.md) for version history and release notes.

## License

[Specify your license here - e.g., MIT, GPL, Apache 2.0]

## Acknowledgments

- Built with ❤️ using .NET 9
- Uses official [Strava API v3](https://developers.strava.com/)
- Uses Hevy API (undocumented V2 endpoints)
- Created to help fitness enthusiasts track complete workout metrics
- Special thanks to the open-source community

## Disclaimer

**Important Legal Notice:**

This application is not officially affiliated with, endorsed by, or connected to Strava, Inc. or Hevy. All product names, logos, and brands are property of their respective owners.

- Use of this application is at your own risk
- The developers are not responsible for any data loss or corruption
- Always verify synchronized data before deleting original workouts
- Ensure compliance with Strava and Hevy's Terms of Service
- API access may be revoked by Strava or Hevy at any time

By using this application, you agree to:
- Strava's [Terms of Service](https://www.strava.com/legal/terms)
- Hevy's Terms of Service
- The terms of this application's license

---

**Made with 💪 for the fitness community**

If you find this application useful, consider:
- ⭐ Starring the repository
- 🐛 Reporting bugs
- 💡 Suggesting features
- 🤝 Contributing code
- 📢 Sharing with friends

# Hevy Heart Rate Synchronizer

A C# .NET 9.0 console application that synchronizes heart rate data from Strava activities to Hevy workouts.

## Features

- **Strava Integration**: OAuth authentication with automatic callback server
- **Hevy Integration**: API integration to fetch and update workouts
- **Heart Rate Synchronization**: Intelligent mapping of Strava heart rate data to match Hevy workout duration
- **Data Export**: Automatic JSON export of both Strava and synchronized data for debugging
- **Interactive Selection**: User-friendly console interface to select workouts from both platforms

## Prerequisites

- .NET 9.0 SDK
- Strava API credentials (Client ID and Secret)
- Hevy API key

## Setup Instructions

### 1. Get Strava API Credentials

1. Go to [Strava Developers](https://developers.strava.com/)
2. Create a new application
3. Note down your **Client ID** and **Client Secret**
4. Set the **Authorization Callback Domain** to `localhost` (for local development)

### 2. Get Hevy API Key

1. Visit the [Hevy API documentation](https://api.hevyapp.com/docs/)
2. Follow their instructions to obtain an API key
3. Note: The application uses Hevy API v1 for fetching workouts and v2 (undocumented) for updating biometrics

### 3. Configure the Application

1. Open `appsettings.json` in the project root
2. Replace the placeholder values with your actual credentials:

```json
{
  "Strava": {
    "ClientId": "YOUR_ACTUAL_STRAVA_CLIENT_ID",
    "ClientSecret": "YOUR_ACTUAL_STRAVA_CLIENT_SECRET",
    "RedirectUri": "http://localhost:8080/callback",
    "Scope": "read,activity:read_all"
  },
  "Hevy": {
    "ApiKey": "YOUR_ACTUAL_HEVY_API_KEY",
    "BaseUrl": "https://api.hevyapp.com"
  },
  "Server": {
    "Port": 8080,
    "Host": "localhost"
  }
}
```

## Usage

### Running the Application

1. Open a terminal in the project directory
2. Build and run the application:
   ```bash
   dotnet build
   dotnet run
   ```

### Application Workflow

1. **Strava Authentication**: 
   - A web browser will open automatically to the Strava authorization page
   - Log in and authorize the application
   - The browser will redirect back to the local callback server

2. **Select Strava Activity**:
   - The application will fetch your recent activities with heart rate data
   - Select the activity you want to synchronize

3. **Select Hevy Workout**:
   - The application will fetch your Hevy workouts
   - Select the workout you want to update with heart rate data

4. **Data Synchronization**:
   - The application will process and synchronize the heart rate data
   - Strava data will be trimmed to match the exact duration of the Hevy workout
   - You'll see a preview of the synchronized data (sample count, calories, HR stats)

5. **Confirmation**:
   - Review the synchronized data and confirm whether to update the Hevy workout
   - The application will attempt to update the workout via the Hevy v2 API

### Data Files

The application automatically creates JSON files for debugging:

- `strava_activity_{id}_{timestamp}.json`: Raw Strava activity and heart rate data
- `synchronized_biometrics_{workout_id}_{timestamp}.json`: Processed data ready for Hevy

## How Heart Rate Synchronization Works

1. **Duration Matching**: The Strava heart rate data is trimmed to exactly match the Hevy workout duration
2. **Timestamp Alignment**: Heart rate samples are created at 1-second intervals using the Hevy workout's start time
3. **Data Interpolation**: If Strava data has different sampling rates, it's intelligently interpolated to match the Hevy workout timeline
4. **Calorie Transfer**: Total calories from Strava are included in the synchronized data

## API Endpoints Used

### Strava API v3
- `GET /athlete/activities` - Fetch user activities
- `GET /activities/{id}` - Get detailed activity information
- `GET /activities/{id}/streams` - Get heart rate stream data
- `POST /oauth/token` - Exchange authorization code for access token

### Hevy API
- `GET /v1/workouts` - Fetch user workouts
- `PUT /v2/workouts/{id}/biometrics` - Update workout biometrics (Note: v2 API is undocumented)

## Troubleshooting

### Common Issues

1. **"Configuration not validated"**: Ensure all API credentials are correctly set in `appsettings.json`

2. **"No activities with heart rate data found"**: Make sure your Strava activities have heart rate data recorded

3. **"Failed to update Hevy workout"**: 
   - Check your Hevy API key permissions
   - The v2 API endpoint may not be available yet - check the generated JSON file for manual upload

4. **Browser doesn't open for Strava auth**: Manually copy and paste the authorization URL shown in the console

5. **Port 8080 already in use**: Change the port in `appsettings.json` under `Server.Port`

### Firewall/Network Issues

- Ensure port 8080 (or your configured port) is not blocked by firewall
- The callback server runs temporarily during OAuth authentication

## Sample Data Structure

The application follows the Hevy JSON structure as shown in the sample file. The key biometrics structure includes:

```json
{
  "biometrics": {
    "total_calories": 629.0,
    "heart_rate_samples": [
      {
        "timestamp_ms": 1755906339000,
        "bpm": 97
      }
      // ... more samples
    ]
  }
}
```

## Notes

- **Hevy v2 API**: The application attempts to use the undocumented Hevy v2 API for updating biometrics. If this fails, you can manually import the generated JSON file
- **Data Privacy**: All heart rate data processing happens locally on your machine
- **Rate Limiting**: The application respects API rate limits but doesn't implement sophisticated retry logic

## Contributing

Feel free to submit issues or pull requests to improve the application functionality or add support for additional APIs.

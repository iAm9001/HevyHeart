# HevyHeart GUI

A modern Windows Presentation Foundation (WPF) graphical user interface for the HevyHeart application. This GUI provides an intuitive way to synchronize heart rate data from Strava activities to Hevy workouts.

## Features

- ?? Modern, clean Material Design-inspired interface
- ?? Easy authentication with Strava and Hevy
- ?? Visual selection of activities and workouts
- ? Real-time status updates and progress indicators
- ?? Synchronization summary with heart rate statistics
- ?? Automatic data refresh after operations

## Prerequisites

- Windows 10 or Windows 11
- .NET 9 SDK or Runtime
- Valid Strava API credentials (Client ID and Client Secret)
- Hevy API Key
- Hevy account credentials

## Installation

### Option 1: Build from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/iAm9001/HevyHeart.git
   cd HevyHeart
   ```

2. Open `HevyHeart.sln` in Visual Studio 2022 or later

3. Set `HevyHeartGui` as the startup project

4. Build the solution (Ctrl+Shift+B)

5. Run the application (F5)

### Option 2: Run Published Application

1. Publish the application:
   ```bash
   dotnet publish HevyHeartGui -c Release -r win-x64 --self-contained
   ```

2. Navigate to the publish folder:
   ```
   HevyHeartGui\bin\Release\net9.0-windows\win-x64\publish\
   ```

3. Run `HevyHeartGui.exe`

## Configuration

Before using the application, you need to configure your API credentials in `appsettings.json`:

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
    "AuthToken": "",
    "EmailOrUsername": "YOUR_HEVY_EMAIL_OR_USERNAME",
    "Password": "YOUR_HEVY_PASSWORD"
  },
  "Server": {
    "Port": 8080,
    "Host": "localhost"
  }
}
```

### Getting API Credentials

Refer to the main [README.md](../README.md) for detailed instructions on obtaining:
- Strava API credentials
- Hevy API key
- Setting up authentication

## Usage

### 1. Launch the Application

Run `HevyHeartGui.exe` or press F5 in Visual Studio.

### 2. Authenticate with Services

#### Hevy Authentication
1. Enter your Hevy email/username and password in the right panel
2. Click "?? Authenticate with Hevy"
3. Wait for the success message
4. Your workouts will load automatically

#### Strava Authentication
1. Click "?? Authenticate with Strava" in the left panel
2. A browser window will open
3. Log in to Strava and authorize the application
4. Return to the app - authentication completes automatically
5. Your activities will load automatically

### 3. Select Activity and Workout

#### Select Strava Activity
1. Browse the list of activities in the left panel
2. Click on an activity to select it
3. Click "?? Load Activity Details" to fetch heart rate data

#### Select Hevy Workout
1. Browse the list of workouts in the right panel
2. Click on a workout to select it
3. Details will load automatically

### 4. Synchronize Heart Rate Data

1. Once both an activity and workout are selected, the "?? Synchronize Heart Rate Data" button becomes enabled
2. Click the button to start synchronization
3. Review the synchronization summary
4. Confirm the operation when prompted
5. Choose whether to delete the old workout (recommended after verifying the new one)

## User Interface Guide

### Main Window Layout

```
???????????????????????????????????????????????????
?           HevyHeart - Header                    ?
???????????????????????????????????????????????????
?   Strava Activities  ?    Hevy Workouts        ?
?                      ?                          ?
?  [Authentication]    ?   [Authentication]       ?
?                      ?                          ?
?  ????????????????    ?   ????????????????      ?
?  ? Activity 1   ?    ?   ? Workout 1    ?      ?
?  ? Activity 2   ?    ?   ? Workout 2    ?      ?
?  ? Activity 3   ?    ?   ? Workout 3    ?      ?
?  ????????????????    ?   ????????????????      ?
?                      ?                          ?
?  [Load Details]      ?                          ?
???????????????????????????????????????????????????
?         Synchronization Summary                 ?
?         Status Bar                              ?
?         [?? Synchronize Heart Rate Data]       ?
???????????????????????????????????????????????????
```

### Visual Indicators

- ? Green checkmark: Success/Authenticated
- ? Red X: Error/Failed
- ?? Spinning icon: Loading/Processing
- ?? Chart icon: Data/Statistics
- ?? Lock icon: Authentication required
- ?? Strong arm: Hevy-related
- ?? Cyclist: Strava-related

### Status Messages

The status bar at the bottom shows:
- Current operation progress
- Success/failure messages
- Error details
- Loading indicators

## Features in Detail

### Authentication

**Hevy Authentication:**
- Credentials can be pre-configured in `appsettings.json` for auto-login
- Manual entry supported through the UI
- Secure password input (masked)
- Session persists for the application lifetime

**Strava Authentication:**
- OAuth 2.0 flow with browser-based authorization
- Automatic callback handling via local server
- Token management handled internally
- Re-authentication required each session for security

### Data Display

**Strava Activities:**
- Activity name
- Date and time
- Activity type (Run, Ride, Workout, etc.)
- Duration
- Average heart rate

**Hevy Workouts:**
- Workout title
- Date and time
- Number of exercises
- Description (if available)

### Synchronization Summary

After processing, displays:
- Number of heart rate samples generated
- Total calories
- Minimum heart rate
- Average heart rate
- Maximum heart rate

## Troubleshooting

### Application Won't Start

**Error: "Could not load file or assembly..."**
- Ensure .NET 9 runtime is installed
- Try repairing .NET installation
- Rebuild the solution in Visual Studio

**Error: "appsettings.json not found"**
- Ensure `appsettings.json` is in the same folder as the executable
- Check that file properties have "Copy to Output Directory" set to "PreserveNewest"

### Authentication Issues

**Hevy authentication fails:**
- Verify credentials in `appsettings.json` are correct
- Check that API key is valid
- Ensure you have internet connectivity
- For Google/Apple sign-in users, verify you've created a password for API access

**Strava authentication fails:**
- Check that port 8080 is not in use
- Verify Client ID and Client Secret are correct
- Ensure redirect URI matches Strava API settings
- Temporarily disable firewall/antivirus

**Browser doesn't open for Strava:**
- Manually copy the URL from error message
- Paste into browser
- Complete authorization
- App will detect the callback automatically

### Data Loading Issues

**Activities or workouts don't load:**
- Check internet connection
- Verify you're authenticated with both services
- Click the refresh button to retry
- Check API rate limits haven't been exceeded

**"Failed to load activity details":**
- Ensure the activity has heart rate data
- Try selecting a different activity
- Check Strava API permissions

### Synchronization Issues

**Synchronize button is disabled:**
- Ensure both an activity and workout are selected
- Verify activity details have been loaded
- Check authentication status for both services

**Synchronization fails:**
- Verify Hevy API key has write permissions
- Check internet connection
- Review error message for specific details
- Try the console application for more detailed logs

### UI Issues

**Window too small/large:**
- Resize the window manually
- Window size is adjustable
- Minimum size is enforced for usability

**Lists are empty after authentication:**
- Click the reload button (??)
- Check status bar for error messages
- Verify API credentials are correct

**Password field doesn't work:**
- Try copy/paste if typing doesn't work
- Check keyboard layout (US English recommended)
- Restart the application

## Architecture

The GUI application follows MVVM (Model-View-ViewModel) pattern:

### Project Structure

```
HevyHeartGui/
??? App.xaml                    # Application definition and resources
??? App.xaml.cs                 # Application startup and DI configuration
??? MainWindow.xaml             # Main window UI layout
??? MainWindow.xaml.cs          # Main window code-behind
??? appsettings.json            # Configuration file
??? Commands/
?   ??? RelayCommand.cs         # ICommand implementations
??? Converters/
?   ??? ValueConverters.cs      # Data binding converters
??? ViewModels/
    ??? ViewModelBase.cs        # Base ViewModel with INotifyPropertyChanged
    ??? MainViewModel.cs        # Main application ViewModel
```

### Dependencies

The GUI project references:
- `HevyHeartConsole` - For services and business logic
- `HevyHeartModels` - For data models
- Microsoft.Extensions.Configuration - For appsettings.json
- Microsoft.Extensions.DependencyInjection - For dependency injection

### Design Patterns Used

- **MVVM (Model-View-ViewModel)**: Separation of concerns
- **Dependency Injection**: Service lifetime management
- **Command Pattern**: User interaction handling
- **Observer Pattern**: Data binding with INotifyPropertyChanged
- **Async/Await**: Non-blocking operations

## Advantages Over Console Application

1. **User-Friendly**: No command-line knowledge required
2. **Visual Feedback**: Real-time progress indicators
3. **Easier Selection**: Click to select instead of typing numbers
4. **Multi-Select**: Review options side-by-side
5. **Better Error Handling**: Clear visual error messages
6. **Persistent UI**: Window stays open, no console flickering
7. **Professional Look**: Modern, polished interface

## Future Enhancements

Potential future features:
- [ ] Batch synchronization (multiple activities at once)
- [ ] Activity/workout filtering and search
- [ ] Visual heart rate chart preview
- [ ] Configuration editor within the UI
- [ ] Multi-language support
- [ ] Dark mode theme
- [ ] Workout history tracking
- [ ] Export synchronized data to CSV/JSON
- [ ] Automatic synchronization scheduling

## Known Limitations

- Windows only (WPF is Windows-specific)
- Single workout synchronization at a time
- Requires manual selection of activity and workout
- No offline mode
- Session-based Strava authentication (doesn't persist)

## Contributing

Contributions are welcome! Areas for improvement:
- UI/UX enhancements
- Additional themes
- Performance optimizations
- Bug fixes
- Documentation improvements

## Support

For issues specific to the GUI:
- Check this README first
- Review the main [README.md](../README.md) for API setup
- Open an issue on GitHub with:
  - Steps to reproduce
  - Screenshots if applicable
  - Error messages
  - Windows version and .NET version

## License

Same as the main HevyHeart project.

## Acknowledgments

- Built with WPF and .NET 9
- Material Design color palette inspiration
- Uses services from HevyHeartConsole project
- Community feedback and testing

---

**Made with ?? for fitness enthusiasts who prefer GUIs**

If you find the GUI useful:
- ? Star the repository
- ?? Report bugs
- ?? Suggest features
- ?? Contribute improvements
- ?? Share with friends

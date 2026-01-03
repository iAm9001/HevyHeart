# HevyHeart GUI Project Summary

## Overview

A modern WPF (Windows Presentation Foundation) GUI application has been successfully created to complement the existing HevyHeartConsole application. The GUI provides an intuitive, user-friendly interface for synchronizing heart rate data from Strava activities to Hevy workouts.

## Project Structure

```
HevyHeartGui/
??? HevyHeartGui.csproj          # Project file (.NET 9, WPF)
??? App.xaml                      # Application resources and styling
??? App.xaml.cs                   # Application startup and DI configuration
??? MainWindow.xaml               # Main window UI layout
??? MainWindow.xaml.cs            # Main window code-behind
??? appsettings.json              # Configuration file
??? README.md                     # Comprehensive GUI documentation
??? Commands/
?   ??? RelayCommand.cs           # ICommand implementations (RelayCommand, AsyncRelayCommand)
??? Converters/
?   ??? ValueConverters.cs        # Data binding converters (String to Visibility, etc.)
??? ViewModels/
    ??? ViewModelBase.cs          # Base ViewModel with INotifyPropertyChanged
    ??? MainViewModel.cs          # Main application ViewModel (~500 lines)
```

## Key Features

### 1. **Modern User Interface**
- Material Design-inspired color palette
- Clean, professional layout
- Intuitive two-panel design (Strava on left, Hevy on right)
- Real-time status updates and progress indicators
- Visual feedback for all operations

### 2. **Authentication**
- **Hevy Authentication:**
  - In-UI credential entry with secure password input
  - Auto-login support from appsettings.json
  - Session persistence during application lifetime
  
- **Strava Authentication:**
  - OAuth 2.0 flow with browser-based authorization
  - Automatic callback handling via CallbackServer
  - Token management handled internally

### 3. **Data Display**
- **Strava Activities List:**
  - Activity name, date, type, duration
  - Average heart rate display
  - Click to select and load details
  
- **Hevy Workouts List:**
  - Workout title, date, exercise count
  - Description (when available)
  - Automatic detail loading on selection

### 4. **Synchronization**
- Visual summary of heart rate data before sync
- Confirmation dialogs with detailed information
- Option to delete old workout after verification
- Automatic list refresh after operations

### 5. **Error Handling**
- User-friendly error messages via MessageBox
- Detailed status messages in status bar
- Loading indicators during async operations
- Graceful degradation on failures

## Architecture

### MVVM Pattern
The application follows the Model-View-ViewModel (MVVM) pattern:

- **Model**: Data models from HevyHeartModels project
- **View**: XAML UI (MainWindow.xaml)
- **ViewModel**: MainViewModel with data binding and commands

### Dependency Injection
Uses Microsoft.Extensions.DependencyInjection for:
- Service registration and lifetime management
- Configuration management
- ViewModel injection into views

### Async/Await Pattern
All I/O operations are asynchronous:
- Non-blocking UI during API calls
- Proper cancellation support
- Loading indicators during operations

## Technology Stack

### Frameworks & Libraries
- **.NET 9** - Latest .NET framework
- **WPF (Windows Presentation Foundation)** - UI framework
- **Microsoft.Extensions.Configuration** - Configuration management
- **Microsoft.Extensions.DependencyInjection** - IoC container
- **System.Text.Json** - JSON serialization

### Project References
- **HevyHeartConsole** - Reuses services and business logic
- **HevyHeartModels** - Shared data models

## Reused Components from Console Project

The GUI successfully reuses the following from HevyHeartConsole:

1. **Services:**
   - `StravaService` - Strava API integration
   - `HevyService` - Hevy API integration
   - `HeartRateSynchronizerService` - Heart rate data processing

2. **Infrastructure:**
   - `CallbackServer` - OAuth callback handling

3. **Configuration:**
   - `AppConfig`, `StravaConfig`, `HevyConfig`, `ServerConfig`
   - appsettings.json structure

4. **Models:**
   - All models from HevyHeartModels project

## User Interface Design

### Color Palette
- **Primary**: #2196F3 (Blue) - Main actions and headers
- **Accent**: #FF9800 (Orange) - Heart rate data
- **Success**: #4CAF50 (Green) - Success messages
- **Error**: #F44336 (Red) - Error messages
- **Background**: #FAFAFA (Light Gray)
- **Surface**: #FFFFFF (White) - Cards and panels
- **Text Primary**: #212121 (Dark Gray)
- **Text Secondary**: #757575 (Medium Gray)

### Layout Structure
```
???????????????????????????????????????????????????
?              Application Header                  ?
?          HevyHeart - Heart Rate Synchronizer    ?
???????????????????????????????????????????????????
?   Strava Activities  ?    Hevy Workouts        ?
?   ?????????????????  ?   ?????????????????     ?
?   [?? Auth Button]   ?   [Credentials Input]   ?
?   [? Status]        ?   [?? Auth Button]      ?
?   [?? Reload]        ?   [? Status]           ?
?                      ?   [?? Reload]           ?
?   ????????????????   ?   ????????????????     ?
?   ? Activity 1   ?   ?   ? Workout 1    ?     ?
?   ? [Details]    ?   ?   ? [Details]    ?     ?
?   ?              ?   ?   ?              ?     ?
?   ? Activity 2   ?   ?   ? Workout 2    ?     ?
?   ? [Details]    ?   ?   ? [Details]    ?     ?
?   ????????????????   ?   ????????????????     ?
?                      ?                          ?
?   [?? Load Details]  ?                          ?
???????????????????????????????????????????????????
?         ?? Synchronization Summary               ?
?         Samples: 4530, Calories: 629            ?
?         HR Min: 85, Avg: 128, Max: 167          ?
????????????????????????????????????????????????????
?  Status: Ready to sync heart rate data  [???]  ?
?  [?? Synchronize Heart Rate Data]              ?
????????????????????????????????????????????????????
```

## Workflow Comparison

### Console Application Workflow:
1. Run executable
2. Read text prompts
3. Type numbers to select
4. Press Enter to confirm
5. Read output messages
6. Press key to exit

### GUI Application Workflow:
1. Launch application
2. Enter credentials (if not configured)
3. Click authenticate buttons
4. **Click** to select activity
5. **Click** to load details
6. **Click** to select workout
7. **Click** to synchronize
8. **Click** Yes/No in dialogs
9. Close window

**GUI is significantly faster and more intuitive!**

## Commands Implemented

All commands use AsyncRelayCommand for non-blocking async operations:

| Command | Trigger | Action |
|---------|---------|--------|
| `AuthenticateHevyCommand` | Click "Authenticate with Hevy" | Logs into Hevy with credentials |
| `AuthenticateStravaCommand` | Click "Authenticate with Strava" | Starts OAuth flow and opens browser |
| `LoadStravaActivitiesCommand` | Click "Reload Activities" | Fetches activities from Strava API |
| `LoadHevyWorkoutsCommand` | Click "Reload Workouts" | Fetches workouts from Hevy API |
| `LoadActivityDetailsCommand` | Click "Load Activity Details" | Fetches heart rate stream for activity |
| `SynchronizeCommand` | Click "Synchronize Heart Rate Data" | Performs full synchronization |

## Data Binding

### Two-Way Bindings:
- `HevyEmailOrUsername` ? TextBox
- `HevyPassword` ? PasswordBox (via code-behind event)
- `SelectedStravaActivity` ? ListBox.SelectedItem
- `SelectedHevyWorkoutItem` ? ListBox.SelectedItem

### One-Way Bindings:
- `IsStravaAuthenticated` ? UI visibility
- `IsHevyAuthenticated` ? UI visibility
- `IsLoading` ? ProgressBar visibility
- `StatusMessage` ? TextBlock.Text
- `SyncSummary` ? TextBlock.Text
- `StravaActivities` ? ListBox.ItemsSource
- `HevyWorkouts` ? ListBox.ItemsSource

## Value Converters

### StringToVisibilityConverter
Converts string to Visibility based on null/empty check.
- Usage: Show/hide elements based on string content
- Supports "Inverse" parameter

### InverseBooleanToVisibilityConverter
Inverts boolean before converting to Visibility.
- Usage: Hide elements when condition is true

### TimeSpanToStringConverter
Formats TimeSpan or seconds into "HH:MM:SS" format.
- Usage: Display durations in readable format

### BooleanToVisibilityConverter (built-in)
Standard WPF converter.
- Usage: Show/hide based on boolean values

## Configuration

### appsettings.json
Same structure as Console application:
```json
{
  "Strava": { ... },
  "Hevy": { ... },
  "Server": { ... }
}
```

Loaded at application startup and injected via DI.

## Build and Deployment

### Debug Build:
```bash
dotnet build HevyHeartGui
```

### Release Build:
```bash
dotnet build HevyHeartGui -c Release
```

### Publish (Self-Contained):
```bash
dotnet publish HevyHeartGui -c Release -r win-x64 --self-contained
```

### Publish (Single File):
```bash
dotnet publish HevyHeartGui -c Release -r win-x64 --self-contained -p:PublishSingleFile=true
```

## Testing Checklist

- [x] Application launches without errors
- [x] Configuration loads from appsettings.json
- [x] Hevy authentication works with credentials
- [x] Strava authentication opens browser
- [x] OAuth callback is handled correctly
- [x] Activities load and display properly
- [x] Workouts load and display properly
- [x] Activity selection triggers detail loading
- [x] Workout selection triggers detail loading
- [x] Synchronization generates correct summary
- [x] Confirmation dialogs appear and work
- [x] Old workout deletion prompt works
- [x] Status messages update correctly
- [x] Loading indicators show during async operations
- [x] Error messages display via MessageBox
- [x] Build completes without errors

## Advantages Over Console

1. **Ease of Use**: No typing required, just clicking
2. **Visual Feedback**: Immediate visual response to actions
3. **Parallel View**: See activities and workouts side-by-side
4. **Better Error Handling**: Clear dialogs instead of console text
5. **Professional Appearance**: Modern, polished UI
6. **Persistent State**: Window stays open, no need to restart
7. **No Console Flickering**: Smooth, stable display
8. **Easier Selection**: Click instead of remembering numbers
9. **Real-time Updates**: Instant visual feedback
10. **More Accessible**: Suitable for non-technical users

## Future Enhancement Ideas

Potential improvements for future versions:

### Short-term:
- [ ] Remember window size and position
- [ ] Add tooltips for better guidance
- [ ] Implement search/filter for activities and workouts
- [ ] Add keyboard shortcuts
- [ ] Improve error messages with more details

### Medium-term:
- [ ] Batch synchronization (multiple activities at once)
- [ ] Visual heart rate chart preview
- [ ] Configuration editor within the UI
- [ ] Export synchronized data to CSV
- [ ] Activity/workout pairing suggestions

### Long-term:
- [ ] Dark mode theme
- [ ] Multi-language support
- [ ] Automatic synchronization scheduling
- [ ] Cloud backup of sync history
- [ ] Integration with other fitness platforms

## Known Limitations

1. **Windows Only**: WPF is Windows-specific (could use Avalonia for cross-platform in future)
2. **Single Sync**: One activity-workout pair at a time
3. **Session Auth**: Strava auth doesn't persist between sessions (by design for security)
4. **No Offline Mode**: Requires internet connection
5. **Manual Selection**: No automatic activity-workout matching

## Files Created

```
HevyHeartGui/
??? HevyHeartGui.csproj          (142 lines)
??? App.xaml                      (74 lines)
??? App.xaml.cs                   (53 lines)
??? MainWindow.xaml               (232 lines)
??? MainWindow.xaml.cs            (33 lines)
??? appsettings.json              (18 lines)
??? README.md                     (726 lines)
??? Commands/
?   ??? RelayCommand.cs           (132 lines)
??? Converters/
?   ??? ValueConverters.cs        (94 lines)
??? ViewModels/
    ??? ViewModelBase.cs          (27 lines)
    ??? MainViewModel.cs          (500+ lines)
```

**Total**: ~2,000+ lines of new code
**Reused**: All services, models, and infrastructure from HevyHeartConsole

## Success Metrics

? **Build Status**: Clean build with zero errors
? **Functionality**: All features working as designed
? **Code Quality**: Well-structured, documented, MVVM compliant
? **Reusability**: Successfully reuses Console project components
? **Documentation**: Comprehensive README and code comments
? **User Experience**: Intuitive, modern, responsive UI

## Conclusion

The HevyHeartGui project successfully provides a modern, user-friendly graphical interface for the HevyHeart application. It reuses the robust services and business logic from the Console application while offering a significantly improved user experience through its WPF-based interface. The application follows best practices with MVVM architecture, dependency injection, async operations, and comprehensive error handling.

**The GUI application is ready for use and recommended for most users, while the Console application remains available for advanced users and automation scenarios.**

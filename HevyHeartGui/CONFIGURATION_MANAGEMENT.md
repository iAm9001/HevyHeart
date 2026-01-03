# Configuration Management Feature

## Overview

The HevyHeart GUI application now includes automatic configuration validation and a user-friendly settings dialog. When the application starts, it automatically checks for missing required settings and prompts the user to provide them.

## Features

### 1. Automatic Configuration Validation
On application startup, the system validates the following required settings:
- **Strava Settings**
  - Client ID
  - Client Secret
  - Redirect URI
- **Hevy Settings**
  - API Key
  - Email/Username (Optional - can be entered in the main window)
  - Password (Optional - can be entered in the main window)

### 2. Settings Dialog
If any required settings are missing or empty, a settings dialog window automatically appears with:
- Clear labeling of all required and optional fields
- Secure password input fields for sensitive data (Client Secret, API Key, Password)
- Validation to ensure all fields are filled before saving
- Cancel option (closes the application if user cancels)
- Information about optional Hevy credentials

### 3. Flexible Hevy Authentication
**Hevy credentials (Email/Username and Password) are now optional** in the settings dialog:
- You can leave them blank and use the login form in the main window
- Or you can enter them in settings for automatic authentication on startup
- The main window provides a dedicated login form for Hevy authentication

### 4. Automatic Configuration Persistence
When the user provides settings:
- Values are saved back to `appsettings.json`
- The JSON file is formatted with proper indentation
- Application continues with the updated configuration

## User Experience

### First Run Experience
1. User launches the application
2. If `appsettings.json` has empty required values, the Settings dialog appears
3. User fills in all required fields:
   - Strava Client ID
   - Strava Client Secret (password field)
   - Strava Redirect URI (pre-filled with default)
   - Hevy API Key (password field)
   - Hevy Email/Username
   - Hevy Password (password field)
4. User clicks **Save**
5. Settings are persisted to `appsettings.json`
6. Main application window appears
7. User can begin using the application

### Cancellation Behavior
If the user clicks **Cancel** on the settings dialog:
- A warning message appears: "Application settings are required to continue. The application will now close."
- The application exits gracefully
- No partial or invalid settings are saved

## Technical Implementation

### Components Added

#### 1. ConfigurationService (`HevyHeartGui/Services/ConfigurationService.cs`)
- **Purpose**: Validates configuration and saves settings to file
- **Key Methods**:
  - `ValidateConfiguration()`: Checks if all required settings are present
  - `SaveConfiguration()`: Persists AppConfig to appsettings.json

#### 2. SettingsViewModel (`HevyHeartGui/ViewModels/SettingsViewModel.cs`)
- **Purpose**: ViewModel for the Settings dialog
- **Properties**: Bindable properties for all configuration fields
- **Commands**:
  - `SaveCommand`: Validates and saves settings
  - `CancelCommand`: Closes dialog without saving

#### 3. SettingsWindow (`HevyHeartGui/SettingsWindow.xaml` and `.xaml.cs`)
- **Purpose**: WPF dialog for collecting configuration values
- **Features**:
  - Dark theme matching the main application
  - Password fields for sensitive data
  - Validation feedback
  - Responsive layout with ScrollViewer

### Application Startup Flow

Modified `App.xaml.cs` startup sequence:
```
1. Load configuration from appsettings.json
2. Validate all required settings
3. If validation fails:
   a. Create SettingsViewModel with current config
   b. Show SettingsWindow as dialog
   c. If user saves:
      - Update AppConfig with new values
      - Persist to appsettings.json
      - Continue startup
   d. If user cancels:
      - Show warning message
      - Exit application
4. If validation passes:
   - Continue normal startup
5. Show MainWindow
```

## Security Considerations

### Password Fields
Sensitive data is protected using WPF `PasswordBox` controls:
- Strava Client Secret
- Hevy API Key
- Hevy Password

These fields:
- Mask input with bullet characters (•••)
- Do not expose password text in memory longer than necessary
- Are bound to ViewModel through code-behind events

### File Storage
Settings are stored in `appsettings.json` as plain text. Users should:
- **Never commit** `appsettings.json` with real credentials to version control
- Ensure `.gitignore` excludes this file
- Protect the file with appropriate file system permissions

## Configuration File Format

The `appsettings.json` file structure:
```json
{
  "strava": {
    "clientId": "your-client-id",
    "clientSecret": "your-client-secret",
    "redirectUri": "http://localhost:8080/callback",
    "scope": "read,activity:read_all"
  },
  "hevy": {
    "apiKey": "your-api-key",
    "baseUrl": "https://api.hevyapp.com",
    "authToken": null,
    "emailOrUsername": "your-email",
    "password": "your-password"
  },
  "server": {
    "port": 8080,
    "host": "localhost"
  }
}
```

**Note**: Property names use camelCase when serialized.

## Validation Rules

The following fields must be non-null and non-empty:
- `Strava.ClientId`
- `Strava.ClientSecret`
- `Strava.RedirectUri`
- `Hevy.ApiKey`

Fields that are optional:
- `Hevy.EmailOrUsername` (can be entered in the main window login form)
- `Hevy.Password` (can be entered in the main window login form)
- `Strava.Scope` (has default value)
- `Hevy.BaseUrl` (has default value)
- `Hevy.AuthToken` (optional)
- `Server.Port` (has default value)
- `Server.Host` (has default value)

## Future Enhancements

Potential improvements for future versions:
1. **Settings Menu**: Add a menu item in MainWindow to open settings anytime
2. **Credential Encryption**: Encrypt sensitive values in appsettings.json
3. **Environment Variables**: Support reading settings from environment variables
4. **Cloud Storage**: Option to store settings in secure cloud storage
5. **Import/Export**: Allow users to import/export settings (excluding passwords)
6. **Validation Feedback**: Real-time validation with specific error messages per field
7. **Test Connection**: Buttons to test Strava/Hevy connections before saving

## Troubleshooting

### Issue: Settings Dialog Appears Every Time
**Cause**: Settings are not being saved properly
**Solution**: 
- Check that the application has write permissions to the directory
- Verify `appsettings.json` exists in the application directory
- Check for file system errors in Windows Event Viewer

### Issue: Settings Not Persisting After Save
**Cause**: Configuration file might be read-only or in a protected directory
**Solution**:
- Check file properties (read-only flag)
- Run application with appropriate permissions
- Move application to a user-writable directory

### Issue: Invalid JSON Format Error
**Cause**: Manual editing of appsettings.json introduced syntax errors
**Solution**:
- Validate JSON using an online validator
- Delete and let the application recreate the file
- Restore from backup if available

## Developer Notes

### Adding New Required Settings
To add a new required setting:

1. Update the relevant config class (e.g., `StravaConfig`, `HevyConfig`)
2. Add validation in `ConfigurationService.ValidateConfiguration()`
3. Add property to `SettingsViewModel`
4. Add UI element to `SettingsWindow.xaml`
5. Update documentation

### Modifying Validation Logic
The validation logic is centralized in `ConfigurationService.ValidateConfiguration()`. Modify this method to:
- Add/remove required fields
- Implement custom validation rules
- Return specific error messages

### Changing UI Theme
The Settings dialog theme is defined in `SettingsWindow.xaml` resources. Key colors:
- Background: `#1E1E1E` (dark gray)
- Input fields: `#2D2D30` (slightly lighter gray)
- Accent: `#4EC9B0` (teal)
- Primary button: `#0E639C` (blue)

## Testing Checklist

When testing this feature:
- [ ] Fresh install with empty appsettings.json shows settings dialog
- [ ] All fields are required (Save button disabled until filled)
- [ ] Password fields mask input
- [ ] Save button persists settings and continues to main window
- [ ] Cancel button shows warning and exits application
- [ ] Saved settings appear correctly in appsettings.json
- [ ] Application starts normally with valid settings (no dialog)
- [ ] Settings survive application restart
- [ ] Invalid JSON in appsettings.json is handled gracefully

## Related Files

- `HevyHeartGui/App.xaml.cs` - Application startup logic
- `HevyHeartGui/Services/ConfigurationService.cs` - Configuration validation and persistence
- `HevyHeartGui/ViewModels/SettingsViewModel.cs` - Settings dialog ViewModel
- `HevyHeartGui/SettingsWindow.xaml` - Settings dialog UI
- `HevyHeartGui/SettingsWindow.xaml.cs` - Settings dialog code-behind
- `HevyHeartGui/appsettings.json` - Configuration file
- `HevyHeartConsole/Config/AppConfig.cs` - Configuration model classes

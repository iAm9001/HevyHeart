# ?? Quick Start Guide - HevyHeart GUI

Get up and running with HevyHeart GUI in just a few minutes!

## ? 3-Minute Setup

### 1. Prerequisites Check (30 seconds)
- [ ] Windows 10 or 11
- [ ] Internet connection
- [ ] Strava account with recorded activities
- [ ] Hevy account with logged workouts

### 2. Get API Credentials (90 seconds)

#### Strava (60 seconds):
1. Visit https://www.strava.com/settings/api
2. Create an app (any name)
3. Set **Authorization Callback Domain** to: `localhost`
4. Copy your **Client ID** and **Client Secret**

#### Hevy (30 seconds):
1. Get your Hevy API key (from Hevy support or developer portal)
2. Know your Hevy username/email and password
   - If you use Google/Apple login, set a password in Hevy settings first

### 3. Configure (60 seconds)
1. Open `appsettings.json` in the HevyHeartGui folder
2. Replace the placeholder values:
   ```json
   {
     "Strava": {
       "ClientId": "PASTE_YOUR_STRAVA_CLIENT_ID_HERE",
       "ClientSecret": "PASTE_YOUR_STRAVA_CLIENT_SECRET_HERE"
     },
     "Hevy": {
       "ApiKey": "PASTE_YOUR_HEVY_API_KEY_HERE",
       "EmailOrUsername": "your-hevy-email@example.com",
       "Password": "your-hevy-password"
     }
   }
   ```
3. Save the file

### 4. Launch (10 seconds)
1. Double-click `HevyHeartGui.exe`
2. The application opens - you're ready!

---

## ?? First Sync - Step by Step

### Step 1: Authenticate with Hevy (10 seconds)
The app will auto-login if you configured credentials in `appsettings.json`.
- If not, enter your credentials in the right panel
- Click **?? Authenticate with Hevy**
- Wait for ? "Authenticated with Hevy"

### Step 2: Authenticate with Strava (20 seconds)
- Click **?? Authenticate with Strava** in the left panel
- Browser opens automatically
- Click **Authorize** on Strava's page
- Browser shows success - return to the app
- Wait for ? "Authenticated with Strava"

### Step 3: Select Activity (10 seconds)
- Browse the list of Strava activities on the left
- Click on an activity that has heart rate data
- Click **?? Load Activity Details**
- Wait for ? "Loaded details for..."

### Step 4: Select Workout (10 seconds)
- Browse the list of Hevy workouts on the right
- Click on a workout that matches your activity
- Details load automatically
- Wait for ? "Loaded details for..."

### Step 5: Synchronize! (20 seconds)
- The **?? Synchronize Heart Rate Data** button is now enabled
- Click it
- Review the summary (samples, calories, heart rate stats)
- Click **Yes** to confirm
- New workout is created in Hevy! ??

### Step 6: Clean Up (optional, 30 seconds)
- Dialog asks: "Delete old workout?"
- **Recommended**: First verify the new workout in Hevy app
- Then click **Yes** to delete the old workout
- Done!

---

## ?? Example First Sync

**Scenario**: You did a strength training session today and wore your heart rate monitor.

1. **Launch** HevyHeartGui.exe
2. **Auto-login** to Hevy (configured in settings) ?
3. **Click** "Authenticate with Strava" ? browser opens
4. **Authorize** in browser ? return to app ?
5. **See** your activities listed on the left
6. **Click** "Morning Chest & Back - 60 min"
7. **Click** "Load Activity Details" ?
8. **See** your workouts listed on the right
9. **Click** "Upper Body Power"
10. **Click** "Synchronize Heart Rate Data"
11. **Review** summary: 3600 samples, 520 calories, HR 85-165 bpm
12. **Click** "Yes" to confirm
13. **Success!** New workout created
14. **Open** Hevy app on phone to verify
15. **Return** to GUI, click "Yes" to delete old workout
16. **Done!** Your Hevy workout now has heart rate data! ??

**Total time**: ~2 minutes

---

## ?? Troubleshooting Quick Fixes

### "? Strava Client ID not configured"
? Edit `appsettings.json` and paste your Client ID

### "? Failed to authenticate with Hevy"
? Check your email/password in `appsettings.json` or the login form

### Browser doesn't open for Strava
? Copy the URL from any error message and paste it into your browser manually

### "Port 8080 already in use"
? Close other applications or change the port in `appsettings.json` to 8081

### Synchronize button is disabled
? Make sure you've loaded activity details and selected a workout

### Still stuck?
? Check the [full README](README.md) or [Console app README](../README.md)

---

## ?? Pro Tips

1. **Auto-login**: Configure credentials in `appsettings.json` to skip manual login
2. **Verify first**: Always check the new workout in Hevy before deleting the old one
3. **Match activities**: Sync activities that match your workout times for best results
4. **Recent first**: Most recent activities and workouts appear at the top of the lists
5. **Multiple syncs**: Just repeat the process for each activity-workout pair

---

## ?? What's Happening Behind the Scenes?

When you click "Synchronize":

1. ? Fetches your Strava activity with second-by-second heart rate data
2. ? Fetches your Hevy workout with exercise details and timestamps
3. ? Aligns heart rate data to match your workout's exact duration
4. ? Generates heart rate samples at 1-second intervals
5. ? Calculates min/max/average heart rate
6. ? Includes calorie data from Strava
7. ? Creates a new Hevy workout with all this data
8. ? (Optional) Deletes the old workout to avoid duplicates

**Result**: Your Hevy workout now has complete heart rate tracking! ??

---

## ?? Next Steps

### Regular Use:
- Run HevyHeartGui after each workout
- Sync your heart rate data while it's fresh
- Track your cardiovascular progress over time

### Advanced:
- Explore the [Console application](../HevyHeartConsole) for automation
- Review the synchronized JSON files for detailed data
- Contribute improvements via GitHub

### Share:
- ? Star the repository if you find it useful
- ?? Tell other fitness enthusiasts
- ?? Report any issues on GitHub

---

## ? You're All Set!

You now know how to:
- ? Configure HevyHeart GUI
- ? Authenticate with Strava and Hevy
- ? Select activities and workouts
- ? Synchronize heart rate data
- ? Troubleshoot common issues

**Happy syncing! ??????**

---

*For detailed documentation, see [README.md](README.md)*
*For troubleshooting, see [README.md](README.md#troubleshooting)*
*For the Console version, see [Console README](../README.md)*

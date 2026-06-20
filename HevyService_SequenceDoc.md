# HevyService Call Sequence Diagram

## Execution Flow (Top to Bottom)

```
┌─────────────────────────────────────────────────────────────────┐
│                      SEQUENCE DIAGRAM                            │
│                  HevyService Authentication &                    │
│                     Workout Loading Flow                         │
└─────────────────────────────────────────────────────────────────┘

┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌─────────────┐
│  UI Thread  │  │Async Context │  │Service Layer│  │ Data Layer  │
└──────┬──────┘  └──────┬───────┘  └──────┬──────┘  └──────┬──────┘
       │                │                 │                │
  1.   │ External Code                    │                │
       │ (Button Click)                   │                │
       │                                   │                │
  ─────┼──────────────────────────────────┼────────────────┼─────
       │                                   │                │
  2.   ├─► AsyncRelayCommand.Execute      │                │
       │   RelayCommand.cs:122            │                │
       │          │                        │                │
       │          │ async start            │                │
  ─────┼──────────┼────────────────────────┼────────────────┼─────
       │          │                        │                │
  3.   │          ├─► AnonymousMethod__15_12                │
       │          │   MainViewModel.cs:61  │                │
       │          │       │                │                │
       │          │       │ calls          │                │
  ─────┼──────────┼───────┼────────────────┼────────────────┼─────
       │          │       │                │                │
  4.   │          │       ├─► WebLoginHevyAsync [Async]     │
       │          │       │   MainViewModel.cs:292          │
       │          │       │       │                          │
       │          │       │       │ awaits                   │
  ─────┼──────────┼───────┼───────┼──────────────────────────┼─────
       │          │       │       │                          │
  5.   │          │       │       ├─► AuthenticateHevyAsync [Async]
       │          │       │       │   MainViewModel.cs:235   │
       │          │       │       │       │                  │
       │          │       │       │       │ then calls       │
  ─────┼──────────┼───────┼───────┼───────┼──────────────────┼─────
       │          │       │       │       │                  │
  6.   │          │       │       │       ├─► LoadHevyWorkoutsAsync [Async]
       │          │       │       │       │   MainViewModel.cs:392
       │          │       │       │       │       │          │
       │          │       │       │       │       │ awaits   │
  ─────┼──────────┼───────┼───────┼───────┼───────┼──────────┼─────
       │          │       │       │       │       │          │
  7.   │          │       │       │       │       ├─► HevyService.GetWorkoutsAsync
       │          │       │       │       │       │   HevyService.cs:254
       │          │       │       │       │       │          │
       │          │       │       │       │       │   constructs via
       │          │       │       │       │       │   External Code
  ─────┼──────────┼───────┼───────┼───────┼───────┼──────────┼─────
       │          │       │       │       │       │          │
  8.   │          │       │       │       │       │          ├─► HevyWorkoutsResponse()
       │          │       │       │       │       │          │   GetWorkoutsResponse.cs:146
       │          │       │       │       │       │          │
       │          │       │       │       │       │◄─────────┤
  ─────┼──────────┼───────┼───────┼───────┼───────┼──────────┼─────
       │          │       │       │       │       │◄─────────┤
       │          │       │       │       │       │          │
       │          │       │       │       │◄──────┤          │
       │          │       │       │◄──────┤       │          │
       │          │       │◄──────┤       │       │          │
       │          │◄──────┤       │       │       │          │
       │◄─────────┤       │       │       │       │          │
       │          │       │       │       │       │          │
       ▼          ▼       ▼       ▼       ▼       ▼          ▼


Legend:
━━━━━  Direct synchronous call
┄┄┄┄┄  Async call (awaited)
- - -  External/indirect call
```

## Call Sequence Details

### Step 1: External Code (Entry Point)
- **Source**: Button click or command binding trigger
- **Type**: UI event
- **Thread**: UI Thread

### Step 2: AsyncRelayCommand.Execute
- **File**: `HevyHeartGui\Commands\RelayCommand.cs:122`
- **Method**: `Execute(object? parameter)`
- **Type**: Command handler
- **Action**: Initiates async execution
- **Thread**: UI Thread

### Step 3: AnonymousMethod__15_12
- **File**: `HevyHeartGui\ViewModels\MainViewModel.cs:61`
- **Type**: Lambda/Anonymous method
- **Context**: Likely part of constructor auto-authentication
- **Action**: Transitions to async context
- **Thread**: Task/Thread Pool

### Step 4: WebLoginHevyAsync
- **File**: `HevyHeartGui\ViewModels\MainViewModel.cs:292`
- **Method**: `WebLoginHevyAsync()` [Async]
- **Type**: ViewModel method
- **Action**: Initiates web-based login flow
- **Thread**: Async context

### Step 5: AuthenticateHevyAsync
- **File**: `HevyHeartGui\ViewModels\MainViewModel.cs:235`
- **Method**: `AuthenticateHevyAsync()` [Async]
- **Type**: ViewModel method
- **Action**: Handles authentication logic
- **Thread**: Async context
- **Next**: Proceeds to load workouts after authentication

### Step 6: LoadHevyWorkoutsAsync
- **File**: `HevyHeartGui\ViewModels\MainViewModel.cs:392`
- **Method**: `LoadHevyWorkoutsAsync()` [Async]
- **Type**: ViewModel method
- **Action**: Initiates workout loading
- **Thread**: Async context

### Step 7: HevyService.GetWorkoutsAsync
- **File**: `HevyHeartConsole\Services\HevyService.cs:254`
- **Method**: `GetWorkoutsAsync(int?, int?, int?)`
- **Type**: Service layer method
- **Action**: Makes HTTP request to Hevy API
- **Thread**: Async context
- **Returns**: Task<HevyWorkoutsResponse>

### Step 8: HevyWorkoutsResponse Constructor
- **File**: `HevyHeartModels\Hevy\V1\GetWorkoutsResponse.cs:146`
- **Type**: Model constructor
- **Action**: Deserializes and constructs response object
- **Invoked via**: External deserialization code (System.Text.Json)
- **Returns**: HevyWorkoutsResponse instance

## Call Types

- **Direct Calls** (solid arrows): Synchronous method calls
- **Async Calls** (dashed arrows): Asynchronous calls with await
- **External Calls** (dotted arrows): Calls through external frameworks/libraries

## Key Observations

1. **Async Chain**: Steps 4-7 form an async chain where each method awaits the next
2. **Auto-Login**: Step 3 suggests automatic authentication on app startup
3. **Separation of Concerns**: Clear layering from UI → ViewModel → Service → Model
4. **External Dependencies**: Final object construction relies on JSON deserialization

## Thread Transitions

- **UI Thread**: Steps 1-2
- **Async Context**: Steps 3-8
- **Return Path**: Async results flow back up the chain to update UI

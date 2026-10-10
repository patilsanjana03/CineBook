# Learning guide

Read this once before you present the project. If you can explain each section in your own
words, you understand the project. Open the code next to this file while you read.

## 1. How a request travels

1. You click **Sign in** in the WPF app.
2. `LoginViewModel` runs `SubmitCommand`, which calls `ApiClient.LoginAsync`.
3. `ApiClient` sends `POST /api/auth/login` to the API.
4. `AuthController` checks the BCrypt hash and returns a JWT inside `AuthResponse`.
5. `MainViewModel.OnLoggedIn` stores the token on the `ApiClient`; every later request now sends it
   as `Authorization: Bearer <token>`.
6. On the server, the JWT middleware validates the token and fills `User` (the claims), so
   `[Authorize]` and `[Authorize(Roles = "Admin")]` work.

## 2. API concepts to know

- **Controllers and routes**: `[ApiController]` plus `[Route("api/[controller]")]` turns
  `BookingsController` into `/api/bookings`.
- **Dependency injection**: controllers ask for `AppDbContext` in their constructor; `Program.cs`
  registers it. You never write `new AppDbContext()`.
- **EF Core**: models in `Models/` become tables. `AppDbContext.OnModelCreating` adds the two unique indexes.
- **DTOs**: the API never returns `User` or `Booking` entities directly, only DTO records.
- **JWT**: a signed token holding claims (id, name, email, role). The server trusts it because only the
  server knows the signing key.
- **Double booking**: the controller checks first for a friendly message, and the unique index on
  `(ShowId, SeatRow, SeatNumber)` is the real guarantee if two people click at the same time.
- **Security detail**: the user id is read from the token, not the request body, so nobody can book as someone else.

## 3. WPF and MVVM concepts to know

- **MVVM**: View (XAML) shows things, ViewModel holds state and commands, Model is the data.
  Views know nothing about the API; view models know nothing about buttons.
- **Data binding**: `Text="{Binding Email}"` keeps the TextBox and the `Email` property in sync.
- **INotifyPropertyChanged**: `ObservableObject.SetProperty` raises the event so the UI refreshes.
- **ICommand**: `RelayCommand` and `AsyncRelayCommand` let a button call view model code.
- **Navigation**: `MainViewModel.CurrentViewModel` changes, and the `DataTemplate`s in
  `MainWindow.xaml` pick the matching View. No Frame or Page needed.
- **ObservableCollection**: tells the list to update when items are added or removed.
- **async/await**: API calls never block the UI thread, so the window stays responsive.

## 3b. The one trick worth remembering

In `SeatSelectionView.xaml` the seat grid is a plain `ItemsControl` with a `UniformGrid` whose
`Columns` is bound to `SeatsPerRow`. The view model only produces a flat list of seats, and the grid
wraps it into rows. That is why changing the hall size needs no UI changes.

## 4. Questions you may be asked

1. Why DTOs instead of returning entities?
2. What happens if two users book the same seat at the same time?
3. Why do you hash passwords, and why BCrypt?
4. What is the difference between authentication and authorization in this project?
5. Why does `AsyncRelayCommand` disable the button while running?
6. Why is the PasswordBox handled in code-behind, and does that break MVVM?
7. Why does `ApiClient` use one shared `HttpClient`?
8. How would you add an admin screen to the desktop app?

## 5. Try changing these (best way to learn)

- Add a `Language` field to `Movie`, then show it in the desktop list.
- Add a "booked on" date to the My bookings screen.
- Add an xUnit test project that tests double booking against an in-memory SQLite database.
- Replace the MessageBox calls with a small dialog service.
- Add an admin screen that calls the admin endpoints.

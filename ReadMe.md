# TaskTrackPro

TaskTrackPro is a robust task management system built with .NET Core MVC, providing functionalities for managing tasks, projects, and user roles with a clean and user-friendly interface.

## Features
- **User Authentication:** Secure login and registration using Identity framework.
- **Role Management:** Different roles for Admin, Project Manager, and User.
- **Task Management:** CRUD operations for tasks with priority levels and deadlines.
- **Project Management:** Creating, updating, and deleting projects.
- **Activity Tracking:** Logs user activities for better traceability.
- **Responsive UI:** Built with Bootstrap and Kendo UI for sleek user experience.

## Technologies Used
- .NET Core MVC
- Kendo UI
- PostgreSQL
- Bootstrap
- C#

## Getting Started

### Prerequisites
- .NET SDK 7.0+
- PostgreSQL
- Visual Studio / Visual Studio Code

### Installation
1. Clone the repository:
   ```bash
   git clone <repository-url>
   ```
2. Navigate to the project directory:
   ```bash
   cd TaskTrackPro
   ```
3. Restore dependencies:
   ```bash
   dotnet restore
   ```
4. Update `appsettings.json` with your PostgreSQL connection string.
5. Apply migrations:
   ```bash
   dotnet ef database update
   ```
6. Run the application:
   ```bash
   dotnet run
   ```

## Usage
- Register or Login as a user.
- Create or manage projects and tasks.
- View project details and task progress.
- Manage user roles if you are an admin.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.

## License
This project is licensed under the MIT License.


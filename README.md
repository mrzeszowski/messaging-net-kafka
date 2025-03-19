
## Prerequisites
Before you start the setup, make sure your environment meets the following requirements:

- Installed .NET version `8.0.406`
- Installed Docker version `27.5.*`
- Installed Node.js version `22.14.*`
- Installed GIT
- Installed IDE (Visual Studio, Rider, Visual Studio Code)

## Setup
In order to prepare your local environment, follow the steps below:

1. Clone GIT repository (`main` branch): https://github.com/mrzeszowski/messaging-net-kafka
2. Start Docker.
3. Go to repository directory using command line tool.
4. Go to `chat-api` directory using command line (`cd ./chat-api`).
5. Run command `docker compose up` in the current directory.
6. Run command `dotnet test` in the current directory. Terminal should print `Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2`.
7. Open `./chat-api/Chat.sln` in your IDE.
8. Start the application in your IDE using `http` launch profile and wait until Swagger UI will open in your browser (url: http://localhost:8092).
9. Run command  `cd ..` in the current directory.
10. Go to `chat-ui` directory using command line (`cd ./chat-ui`).
11. Run command  `npm install` in the current directory.
12. Run command  `npm run dev` in the current directory.
13. Open the application in your browser (url: http://localhost:5173)

Following these steps should build and run the `chat-ui` and `chat-api` applications. For logging in, the application uses `localStorage`. To log in, use your email address @euvic.pl.
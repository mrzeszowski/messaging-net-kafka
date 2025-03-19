
## Prerequisites
Before you start the setup, make sure your environment meets the following requirements:

- Installed .NET version `8.0.406`
- Installed Docker version `27.5.1`
- Installed Node.js version `22.14.0`
- Installed GIT
- Installed IDE (Visual Studio, Rider, Visual Studio Code)

## Setup
In order to prepare your local environment, follow the steps below:

1. Clone GIT repository (`main` branch): https://github.com/mrzeszowski/messaging-net-kafka
1. Start Docker.
1. Go to repository directory using command line tool.
1. Go to `chat-api` directory using command line (`cd ./chat-api`).
1. Run command `docker compose up` in the current directory.
1. Run command `dotnet test` in the current directory. Terminal should print `Passed!  - Failed:     0, Passed:     2, Skipped:     0, Total:     2`.
1. Open `./chat-api/Chat.sln` in your IDE.
1. Start the application in your IDE using `http` launch profile and wait until Swagger UI will open in your browser (url: http://localhost:8092).
1. Run command  `cd ..` in the current directory.
1. Go to `chat-ui` directory using command line (`cd ./chat-ui`).
1. Run command  `npm install` in the current directory.
1. Run command  `npm run dev` in the current directory.
1. Open the application in your browser (url: http://localhost:5173)
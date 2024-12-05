# Contributing Guidelines

## Getting Started

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Submit a pull request

## Development Guidelines

### Code Style
- Follow C# standard naming conventions
- Add XML documentation for public APIs
- Keep methods focused and concise
- Use async/await for asynchronous operations

### Testing
- Write unit tests for new features
- Ensure all tests pass before submitting
- Include integration tests when needed

### Running Unit Tests
- Ensure you have the .NET SDK installed
- Navigate to the project root directory
- Run the following command to execute all unit tests:
  ```bash
  dotnet test
  ```

### Pull Requests
- Use descriptive titles
- Reference any related issues
- Include test coverage
- Update documentation

### Commit Messages
- Use clear, descriptive messages
- Reference issue numbers when applicable

## Project Structure

```
src/
  ├── Controllers/     # API endpoints
  ├── Services/        # Business logic
  ├── Models/          # Data models
  └── Middleware/      # Custom middleware
tests/
  ├── Unit/           # Unit tests
  └── Integration/    # Integration tests
```

## Need Help?
- Check existing issues
- Review documentation
- Ask in discussions

Thank you for contributing!

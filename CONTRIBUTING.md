# Contributing Guide

Thank you for considering contributing to this industrial application project!

## Getting Started

1. Fork the repository
2. Clone your fork: `git clone https://github.com/YOUR_USERNAME/gateway-ads.git`
3. Create a branch: `git checkout -b feature/your-feature-name`
4. Make your changes
5. Test your changes
6. Commit your changes
7. Push to your fork
8. Create a Pull Request

## Development Setup

Follow the [QUICKSTART.md](QUICKSTART.md) guide to set up your development environment.

## Code Style

### JavaScript/React

- Use ES6+ syntax
- Use functional components with hooks
- Follow React best practices
- Use meaningful variable names
- Add comments for complex logic

### Node.js

- Use async/await for asynchronous code
- Handle errors properly
- Use environment variables for configuration
- Follow Express.js best practices

### TwinCAT PLC

- Follow IEC 61131-3 standards
- Use descriptive variable names
- Add comments in French or English
- Use function blocks for reusable logic

## Project Structure

```
/gateway-ads
  /backend         - Node.js API server
  /frontend        - React application
  /plc-simulator   - TwinCAT PLC code
```

## Making Changes

### Backend Changes

1. Create/modify files in `/backend`
2. Test API endpoints
3. Update documentation if API changes
4. Ensure backward compatibility

### Frontend Changes

1. Create/modify files in `/frontend/src`
2. Test in browser
3. Ensure responsive design
4. Check dark theme compatibility
5. Update components documentation

### PLC Changes

1. Modify files in `/plc-simulator`
2. Test in TwinCAT simulator
3. Update variable documentation
4. Ensure ADS compatibility

## Testing

### Backend Testing

```bash
cd backend
npm test  # (when tests are added)
```

### Frontend Testing

```bash
cd frontend
npm test  # (when tests are added)
```

### Manual Testing

1. Start backend and frontend
2. Test all features:
   - Machine connection
   - Recipe creation
   - Process execution
   - PDF generation
   - History viewing
3. Check browser console for errors
4. Check server logs for errors

## Commit Messages

Follow conventional commits format:

```
feat: add new feature
fix: fix bug
docs: update documentation
style: code style changes
refactor: refactor code
test: add tests
chore: maintenance tasks
```

Examples:
```
feat: add ingredient sorting in recipe form
fix: resolve WebSocket reconnection issue
docs: update API documentation for status endpoint
```

## Pull Request Process

1. Ensure your code follows the style guidelines
2. Update documentation if needed
3. Add/update tests if applicable
4. Ensure all tests pass
5. Update CHANGELOG.md
6. Create a clear PR description:
   - What changed
   - Why it changed
   - How to test it

## Areas for Contribution

### Easy (Good First Issues)

- Fix typos in documentation
- Improve error messages
- Add loading states
- Improve UI/UX details
- Add more machine presets

### Medium

- Add input validation
- Improve error handling
- Add unit tests
- Improve chart customization
- Add more process steps

### Hard

- Add authentication
- Add database integration
- Add multiple PLC support
- Add real-time alerts
- Add advanced reporting

## Feature Requests

Open an issue with:
- Clear description
- Use case
- Expected behavior
- Proposed solution (optional)

## Bug Reports

Open an issue with:
- Description of the bug
- Steps to reproduce
- Expected behavior
- Actual behavior
- Screenshots (if applicable)
- Environment details:
  - OS
  - Node.js version
  - Browser (for frontend issues)

## Code Review

All PRs require review before merging:
- Code quality
- Functionality
- Documentation
- Tests (when applicable)

## Community

- Be respectful
- Be constructive
- Help others
- Follow code of conduct

## Questions?

Open an issue with the "question" label or contact the maintainers.

---

Thank you for contributing! 🚀

# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2024-12-06

### Added

#### Frontend
- React.js application with Mantine UI dark theme
- Home page with machine selector and status displays
- New recipe page with dynamic ingredient form
- History page for viewing completed recipes
- Real-time charts using Recharts
- Process timeline visualization
- Machine status monitoring (temperature, pressure, speed)
- Process status monitoring (progress, current step)
- Zustand state management
- WebSocket integration for real-time updates
- React Hook Form for recipe management
- Responsive design for all screen sizes

#### Backend
- Node.js/Express REST API server
- ADS client for TwinCAT PLC communication
- WebSocket server for real-time data streaming
- PDF generation using Puppeteer
- Recipe management endpoints
- Machine management endpoints
- Status monitoring endpoints
- History/report management endpoints
- Mustache templating for PDF reports

#### PLC Simulator
- TwinCAT 3 Structured Text program
- Global Variable Lists (GVLs):
  - GVL_Recipe: Recipe data storage
  - GVL_Process: Process state and progress
  - GVL_Machine: Machine sensor simulation
  - GVL_Command: Control commands
- Function Blocks:
  - FB_ProcessStateMachine: 7-step process automation
  - FB_MachineSimulation: Realistic sensor simulation
- MAIN program with edge detection
- PlcTask configuration (10ms cycle, port 851)

#### Documentation
- Comprehensive README with installation instructions
- QUICKSTART guide for quick setup
- ARCHITECTURE documentation
- API_DOCUMENTATION with complete endpoint reference
- CONTRIBUTING guidelines
- LICENSE (MIT)
- PLC-specific README

#### Project Structure
- Root package.json with convenience scripts
- Environment configuration examples
- .gitignore files for all components
- Project organization following best practices

### Features

#### Recipe Management
- Create recipes with multiple ingredients
- Specify name, quantity, volume, and molar mass for each ingredient
- Dynamic ingredient list with add/remove functionality
- Send recipe data to PLC via ADS

#### Process Control
- Start/stop process execution
- Real-time process monitoring
- 7-step automated process:
  1. Preparation (5s)
  2. Dosing Ingredient A (7s)
  3. Dosing Ingredient B (7s)
  4. Mixing (10s)
  5. Verification (4s)
  6. Finalizing (3s)
  7. Done

#### Real-time Monitoring
- Motor temperature tracking
- Oil pressure monitoring
- Motor speed display
- Warning indicators for out-of-range values
- Progress visualization
- Step-by-step timeline

#### Reporting
- Automatic PDF report generation
- Professional report template
- Recipe details and metadata
- Process step log with sensor data
- Downloadable PDF files
- Historical report access

#### Multi-Machine Support
- Configure multiple machines
- Select active machine from dropdown
- Machine-specific ADS configuration
- Connection status indication

### Technical Details
- React 18 with hooks
- Mantine UI v7
- Express.js web server
- node-ads v4 for ADS protocol
- WebSocket communication at 1Hz
- Puppeteer for PDF generation
- TwinCAT 3 PLC simulation

### Known Limitations
- No authentication/authorization
- Single concurrent PLC connection
- In-memory data storage
- No database integration
- Development-only configuration

---

## [Unreleased]

### Planned Features
- User authentication and authorization
- Database integration (PostgreSQL/MongoDB)
- Multiple simultaneous PLC connections
- Advanced reporting with custom templates
- Real-time alerts and notifications
- Data export (CSV, Excel)
- Process scheduling
- Audit logging
- Role-based access control
- Advanced analytics and insights

### Planned Improvements
- Unit tests for backend
- Component tests for frontend
- E2E tests
- CI/CD pipeline
- Docker containerization
- Production deployment guide
- Performance optimizations
- Error handling improvements
- Input validation and sanitization

---

## Version History

### v1.0.0 (2024-12-06)
- Initial release
- Complete industrial application architecture
- Frontend, backend, and PLC simulator
- Core features implemented
- Documentation complete

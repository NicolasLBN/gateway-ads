# Features Overview

Detailed feature documentation for the Industrial Application.

## 🏠 Home Dashboard

### Machine Selection
- **Multi-Machine Support**: Select from configured machines
- **Connection Management**: Connect/disconnect to PLC
- **Connection Status**: Real-time visual indicator (green/red)
- **Machine Details**: View AMS Net ID, Port, and Location

### Real-time Monitoring
- **Motor Temperature**: Live temperature display with warning threshold
- **Oil Pressure**: Pressure monitoring in bar
- **Motor Speed**: RPM display with visual gauge
- **Warning Indicators**: Color-coded badges for out-of-range values

### Process Status
- **Current Step**: Display active process step
- **Overall Progress**: Percentage completion of entire process
- **Step Progress**: Percentage completion of current step
- **Time Tracking**: Step time and total process time
- **Error Display**: Show error codes and messages
- **Completion Status**: Visual indicator when process done

## 📝 Recipe Management

### Recipe Creation
- **Dynamic Form**: Add/remove ingredients dynamically
- **Ingredient Properties**:
  - Name (text)
  - Quantity (grams)
  - Volume (milliliters)
  - Molar Mass (g/L)
- **Form Validation**: Ensure required fields are filled
- **Multi-Ingredient Support**: Up to 10 ingredients per recipe

### Recipe Operations
- **Send to PLC**: Transfer recipe data via ADS
- **Start Process**: Initiate automated process execution
- **Real-time Monitoring**: Watch process progress live
- **Reset Process**: Return to idle state
- **Generate Report**: Create PDF after completion

## 📊 Real-time Visualization

### Process Timeline
- **Step Visualization**: See all 7 process steps
- **Progress Indicator**: Visual markers for completed/active/pending steps
- **Step Names**: French labels for each step
- **Color Coding**: 
  - Green: Completed
  - Blue: Active
  - Gray: Pending

### Live Charts
- **Multi-Line Chart**: Display 4 simultaneous data streams
- **Temperature Plot**: Motor temperature over time
- **Pressure Plot**: Oil pressure tracking
- **Speed Plot**: Motor speed (scaled for visibility)
- **Progress Plot**: Overall process completion
- **Auto-Scrolling**: Last 100 data points displayed
- **Legend**: Clear identification of each line
- **Dark Theme**: Industrial aesthetic

## 📋 History & Reports

### Report Management
- **List View**: All completed recipes in table format
- **Sorting**: Chronological order (newest first)
- **Search**: Find specific recipes
- **Details Display**:
  - Recipe name
  - Machine used
  - Date and time
  - List of products
- **Download**: One-click PDF download

### PDF Reports
- **Professional Layout**: Clean, industrial template
- **Metadata Section**:
  - Recipe name
  - Date and time
  - Machine name
  - List of products with quantities
- **Process Log Table**:
  - Step names
  - Duration (seconds)
  - Motor temperature
  - Oil pressure
  - Motor speed
  - Remarks/warnings
- **Print-Ready**: A4 format with margins

## 🔌 PLC Integration

### ADS Communication
- **Protocol**: ADS (Automation Device Specification)
- **Read Operations**: 
  - Machine sensor data (temperature, pressure, speed)
  - Process status (step, progress, errors)
- **Write Operations**:
  - Recipe data
  - Control commands (start, reset)
- **Update Rate**: 1 Hz (every second)
- **Connection Management**: Auto-reconnect on disconnect

### TwinCAT 3 Simulator
- **7-Step Process**:
  1. Idle: Waiting for start
  2. Preparation: 5 seconds
  3. Dosing Ingredient A: 7 seconds
  4. Dosing Ingredient B: 7 seconds
  5. Mixing: 10 seconds
  6. Verification: 4 seconds
  7. Finalizing: 3 seconds
  8. Done: Process complete

- **Realistic Simulation**:
  - Temperature increases during process
  - Pressure varies with motor speed
  - Speed changes per step
  - Random anomalies (1% chance)
  - Cooling when idle

## 🎨 User Interface

### Design System
- **Framework**: Mantine UI v7
- **Theme**: Dark industrial
- **Color Scheme**: High contrast
- **Typography**: Arial, sans-serif
- **Responsive**: Mobile, tablet, desktop

### Components
- **Cards**: Rounded corners, shadows, borders
- **Buttons**: Clear call-to-action styling
- **Forms**: Intuitive input fields
- **Tables**: Sortable, hoverable rows
- **Badges**: Status indicators
- **Progress Bars**: Visual feedback
- **Alerts**: Success/error messages
- **Loading States**: Spinners and skeleton screens

### Navigation
- **Header**: Always visible with navigation links
- **Breadcrumbs**: Current location context
- **Back Buttons**: Easy navigation
- **Active States**: Highlight current page

## 📡 Real-time Communication

### WebSocket
- **Protocol**: ws://
- **Connection**: Auto-connect on page load
- **Reconnection**: Automatic retry on disconnect
- **Update Rate**: 1 Hz (1 message per second)
- **Message Types**:
  - Status updates
  - Error notifications
  - Acknowledgments

### Data Streaming
- **Push-Based**: Server pushes data to clients
- **Low Latency**: Near real-time updates
- **Efficient**: Only changed data transmitted
- **Reliable**: Error handling and retry logic

## 🔧 Configuration

### Environment Variables
- **Backend**:
  - PORT: Server port (default: 3001)
  - AMS_NET_ID: PLC network ID
  - AMS_PORT: PLC port (default: 851)
- **Frontend**:
  - REACT_APP_API_URL: Backend API URL
  - REACT_APP_WS_URL: WebSocket URL

### Machine Configuration
- **Preset Machines**: 3 configured by default
- **Custom Configuration**: Add via backend
- **Per-Machine Settings**:
  - Name
  - AMS Net ID
  - AMS Port
  - Location

## 🔒 Safety Features

### Warning System
- **Temperature Warnings**: Alert when >65°C
- **Pressure Warnings**: Alert when <2.0 or >5.0 bar
- **Speed Warnings**: Alert when >2000 RPM
- **Visual Indicators**: Color-coded badges
- **Process Interruption**: Stop on critical errors

### Error Handling
- **Connection Errors**: Clear messages
- **Validation Errors**: Form-level feedback
- **API Errors**: User-friendly messages
- **PLC Errors**: Display error codes and text
- **Graceful Degradation**: App works without PLC

## 📈 Data Management

### State Management
- **Global Store**: Zustand for state
- **Persistent Data**: LocalStorage for preferences
- **Cache Strategy**: In-memory for active data
- **History Storage**: File-based JSON

### Data Flow
1. User creates recipe
2. Frontend validates input
3. API sends to backend
4. Backend writes to PLC via ADS
5. PLC executes process
6. Backend reads data via ADS
7. WebSocket pushes to frontend
8. Frontend updates UI
9. User generates report
10. Backend creates PDF
11. User downloads report

## 🚀 Performance

### Optimization
- **Lazy Loading**: Routes loaded on demand
- **Memoization**: Expensive computations cached
- **Debouncing**: Real-time updates throttled
- **Virtual Scrolling**: Large lists optimized

### Scalability
- **Modular Architecture**: Easy to extend
- **Separation of Concerns**: Clean code structure
- **API Design**: RESTful and consistent
- **Database-Ready**: Can add persistence layer

## 🛠️ Developer Experience

### Code Quality
- **TypeScript-Ready**: Can add types
- **ESLint-Compatible**: Linting support
- **Prettier-Compatible**: Code formatting
- **Commented**: Critical sections documented

### Development Tools
- **Hot Reload**: Frontend auto-refresh
- **Nodemon**: Backend auto-restart
- **Browser DevTools**: React DevTools support
- **Error Messages**: Descriptive and helpful

### Testing Strategy
- **Unit Tests**: Component and function tests
- **Integration Tests**: API endpoint tests
- **E2E Tests**: Full workflow tests
- **Manual Testing**: UI/UX validation

## 🌐 Browser Support

### Compatibility
- **Chrome**: ✅ Full support
- **Firefox**: ✅ Full support
- **Safari**: ✅ Full support
- **Edge**: ✅ Full support
- **Mobile Browsers**: ✅ Responsive design

### Requirements
- **JavaScript**: ES6+ support required
- **WebSocket**: Native support required
- **CSS**: Grid and Flexbox support
- **Modern APIs**: Fetch, Promise, async/await

## 📱 Accessibility

### Standards
- **WCAG 2.1**: Level AA compliance (future goal)
- **Semantic HTML**: Proper element usage
- **ARIA Labels**: Screen reader support
- **Keyboard Navigation**: Full keyboard access
- **Color Contrast**: High contrast for readability

### Inclusive Design
- **Responsive**: Works on all screen sizes
- **Font Scaling**: Respects user preferences
- **Error Messages**: Clear and helpful
- **Loading States**: Visual feedback

## 🔮 Future Enhancements

### Planned Features
- User authentication
- Database integration
- Advanced analytics
- Email notifications
- Mobile app
- Offline support
- Multi-language support
- Advanced reporting
- Data export
- API webhooks

### Community Requests
- Custom process templates
- Recipe sharing
- Machine groups
- Scheduled processes
- Alert configuration
- Custom themes
- Plugin system

---

See [CHANGELOG.md](CHANGELOG.md) for version history and [API_DOCUMENTATION.md](API_DOCUMENTATION.md) for technical details.

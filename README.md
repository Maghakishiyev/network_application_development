# Currency Exchange Office Application

A comprehensive currency exchange platform built with .NET technologies, featuring a WCF service backend, console client, and cross-platform MAUI mobile application.

## Project Overview

This application allows users to:
- View current and historical currency exchange rates from NBP API (National Bank of Poland)
- Check gold prices
- Register and authenticate users
- Manage currency balances
- Buy and sell currencies
- Track transaction history

## Architecture

The solution consists of four main projects:

### CurrencyService
- CoreWCF-based SOAP service
- Integrates with [NBP API](https://api.nbp.pl/) for real-time currency data
- Connects to MongoDB for data persistence
- Provides comprehensive API for currency operations, user management, and trading

### CurrencyData
- Contains shared data models and MongoDB repositories
- Implements data access logic for user accounts, balances, and transactions
- Defines DTOs (Data Transfer Objects) for service communication

### CurrencyClient
- Console application client for the WCF service
- Allows testing of all service operations
- Provides command-line interface for currency operations

### CurrencyMobile
- .NET MAUI cross-platform mobile application
- Targets Android and iOS platforms
- Features MVVM architecture
- Includes:
  - User authentication (login and registration)
  - Currency rates display
  - Trading interface
  - Account balance overview
  - Transaction history

## Getting Started

### Prerequisites
- .NET 8.0 SDK
- MongoDB (local or remote instance)
- Visual Studio 2022 or Visual Studio Code
- Android SDK or iOS development tools (for mobile app)

### Configuration

1. Update the connection string in `CurrencyService/appsettings.json` to point to your MongoDB instance

2. Configure service endpoints:
   - For development: Uses `http://0.0.0.0:5001` and `https://0.0.0.0:7074`
   - For Android emulator connection: Special IP `10.0.2.2` is used in the mobile app

### Running the Application

#### Running the Service
```bash
cd CurrencyService
dotnet run
```

#### Running the Console Client
```bash
cd CurrencyClient
dotnet run
```

#### Running the Mobile App
For Android:
```bash
cd CurrencyMobile
dotnet build -t:Run -f net8.0-android
```

For iOS:
```bash
cd CurrencyMobile
dotnet build -t:Run -f net8.0-ios
```

## Features

### Currency Data
- Current exchange rates for multiple currencies
- Historical exchange rates with date range selection
- Buy/Sell rates (bid/ask prices)
- Current and historical gold prices

### User Management
- User registration with email and password
- Secure authentication with SHA-256 password hashing
- Account balance management

### Trading Operations
- Currency purchase (exchange PLN for foreign currency)
- Currency selling (exchange foreign currency for PLN)
- PLN deposits for initial balance
- Error checking for insufficient funds

### Mobile Application
- Responsive UI that works across device sizes
- Secure credential storage
- Offline capability for viewing previous transaction history
- Real-time currency rate updates

## Database Schema

### Users Collection
Stores user authentication data:
- Email (unique identifier)
- Username
- Password hash (SHA-256)
- Creation timestamp

### UserBalances Collection
Stores currency balances for each user:
```json
{
  "_id": ObjectId("..."),
  "UserId": ObjectId("..."),
  "Currencies": {
    "PLN": 612.833,
    "USD": 49.764,
    "EUR": 46.852
  }
}
```

### Transactions Collection
Records trading history:
```json
{
  "_id": ObjectId("..."),
  "UserId": ObjectId("..."),
  "Type": "buy|sell",
  "CurrencyCode": "USD",
  "Amount": 79.764,
  "Timestamp": ISODate("2025-04-27T11:03:03.392Z")
}
```

## NBP API Integration

The application integrates with the National Bank of Poland's API:
- Base URL: https://api.nbp.pl/api/
- Endpoints used:
  - Current rate: `/exchangerates/rates/a/{code}/`
  - Historical rates: `/exchangerates/rates/a/{code}/{startDate}/{endDate}/`
  - Buy/Sell rates: `/exchangerates/rates/c/{code}/`
  - Current gold price: `/cenyzlota/`
  - Historical gold prices: `/cenyzlota/{startDate}/{endDate}/`

## Technical Details

### WCF Service Implementation
- Uses CoreWCF for modern .NET implementation
- Configured with BasicHttpBinding for broad client compatibility
- Exposes comprehensive API for all currency operations

### Mobile App Implementation
- Built with .NET MAUI for cross-platform compatibility
- Uses MVVM pattern with data binding
- Custom converters for UI presentation
- AppShell-based navigation

### Android-macOS Connectivity
- Special configuration for Android emulator to connect to macOS host
- Network security configuration to allow cleartext traffic in development
- Uses `10.0.2.2` special IP from Android emulator to connect to host machine

## Future Enhancements
- Password reset functionality
- User profile management
- HTTPS for production environment
- Comprehensive logging and telemetry
- Performance optimizations for larger datasets
- Additional currencies and trading pairs
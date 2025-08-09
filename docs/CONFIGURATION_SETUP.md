# School District Billing System - Configuration & Setup Documentation

## Environment Configuration

### Development Environment Setup

#### Prerequisites
- **.NET 8.0 SDK** - Download from [Microsoft .NET](https://dotnet.microsoft.com/download)
- **MySQL Server 8.0+** - Download from [MySQL](https://dev.mysql.com/downloads/mysql/)
- **Visual Studio 2022** or **Visual Studio Code** with C# extension
- **Git** for version control

#### Database Configuration

##### MySQL Server Setup
1. **Install MySQL Server 8.0+**
   ```bash
   # macOS with Homebrew
   brew install mysql
   brew services start mysql
   
   # Windows - Download installer from MySQL website
   # Linux (Ubuntu)
   sudo apt update
   sudo apt install mysql-server
   sudo systemctl start mysql
   ```

2. **Create Database and User**
   ```sql
   -- Connect to MySQL as root
   mysql -u root -p
   
   -- Create database
   CREATE DATABASE omni_test CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   
   -- Create application user
   CREATE USER 'billing_user'@'localhost' IDENTIFIED BY 'secure_password_here';
   
   -- Grant permissions
   GRANT ALL PRIVILEGES ON omni_test.* TO 'billing_user'@'localhost';
   FLUSH PRIVILEGES;
   
   -- Verify connection
   mysql -u billing_user -p omni_test
   ```

3. **Connection String Configuration**
   Update `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "Db": "Server=localhost;Database=omni_test;Uid=billing_user;Pwd=secure_password_here;CharSet=utf8mb4;"
     }
   }
   ```

#### Application Configuration Files

##### appsettings.json (Base Configuration)
```json
{
  "ConnectionStrings": {
    "Db": "Server=localhost;Database=omni_test;Uid=billing_user;Pwd=your_password"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "Microsoft.Hosting.Lifetime": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Warning"
    }
  },
  "AllowedHosts": "*",
  "FileSettings": {
    "TempDirectory": "wwwroot/temp",
    "ArchiveDirectory": "wwwroot/archive",
    "ReportTemplateDirectory": "wwwroot/reportTemplates",
    "MaxFileSize": 52428800,
    "AllowedExtensions": [".xlsx", ".xls"]
  },
  "ReportSettings": {
    "DefaultPageSize": 50,
    "MaxReportRows": 10000,
    "ReportRetentionDays": 30
  }
}
```

##### appsettings.Development.json (Development Overrides)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  },
  "ConnectionStrings": {
    "Db": "Server=localhost;Database=omni_dev;Uid=dev_user;Pwd=dev_password"
  },
  "DetailedErrors": true,
  "FileSettings": {
    "TempDirectory": "wwwroot/temp",
    "CleanupTempFiles": false
  }
}
```

##### appsettings.Production.json (Production Configuration)
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Error",
      "Microsoft.Hosting.Lifetime": "Information"
    }
  },
  "ConnectionStrings": {
    "Db": "Server=prod-mysql-server;Database=billing_production;Uid=prod_user;Pwd=SECURE_PRODUCTION_PASSWORD"
  },
  "AllowedHosts": "yourdomain.com,www.yourdomain.com",
  "FileSettings": {
    "TempDirectory": "/var/app/temp",
    "ArchiveDirectory": "/var/app/archive",
    "ReportTemplateDirectory": "/var/app/templates",
    "CleanupTempFiles": true,
    "CleanupIntervalMinutes": 60
  },
  "Security": {
    "RequireHttps": true,
    "HstsMaxAge": 31536000
  }
}
```

### Database Migration and Setup

#### Initial Database Setup
```bash
# Navigate to project directory
cd SchoolDistrictBilling

# Install Entity Framework tools (if not already installed)
dotnet tool install --global dotnet-ef

# Create initial migration
dotnet ef migrations add InitialCreate

# Apply migration to database
dotnet ef database update

# Verify database was created
mysql -u billing_user -p omni_test -e "SHOW TABLES;"
```

#### Adding New Migrations
```bash
# After making model changes
dotnet ef migrations add DescriptiveNameForChanges

# Review the generated migration file
# Apply to database
dotnet ef database update

# Rollback if needed
dotnet ef database update PreviousMigrationName
```

### File System Configuration

#### Directory Structure Setup
```bash
# Ensure required directories exist
mkdir -p wwwroot/temp
mkdir -p wwwroot/archive
mkdir -p wwwroot/reports
mkdir -p wwwroot/reportTemplates

# Set appropriate permissions (Linux/macOS)
chmod 755 wwwroot/temp
chmod 755 wwwroot/archive
chmod 755 wwwroot/reports

# Windows - ensure IIS_IUSRS has write permissions to these directories
```

#### Report Template Configuration
Place Excel templates in `wwwroot/reportTemplates/`:
- `MonthlyInvoice.xlsx` - Monthly billing invoice template
- `YearEndReconciliation.xlsx` - Year-end reconciliation template
- `MonthlyIndividualStudent.xlsx` - Individual student report template
- `ReconIndividualStudent.xlsx` - Reconciliation student template

### Environment-Specific Configuration

#### Development Environment
```csharp
// Program.cs - Development-specific configuration
public static void Main(string[] args)
{
    var builder = WebApplication.CreateBuilder(args);
    
    if (builder.Environment.IsDevelopment())
    {
        // Development-specific services
        builder.Services.AddDeveloperExceptionPage();
        
        // Enable detailed Entity Framework logging
        builder.Services.AddDbContext<AppDbContext>(options =>
        {
            options.UseMySQL(builder.Configuration.GetConnectionString("Db"));
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }
    
    var app = builder.Build();
    
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    
    app.Run();
}
```

#### Production Environment
```csharp
// Production-specific configuration
if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
    // Security headers
    app.Use(async (context, next) =>
    {
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        await next();
    });
}
```

### Security Configuration

#### Connection String Security
```csharp
// Use environment variables for sensitive data
public void ConfigureServices(IServiceCollection services)
{
    var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
                          ?? Configuration.GetConnectionString("Db");
    
    services.AddDbContext<AppDbContext>(options =>
        options.UseMySQL(connectionString));
}
```

#### Environment Variables Setup
```bash
# Linux/macOS - Add to ~/.bashrc or ~/.zshrc
export DATABASE_CONNECTION_STRING="Server=localhost;Database=omni_prod;Uid=prod_user;Pwd=SECURE_PASSWORD"

# Windows - Set system environment variable
setx DATABASE_CONNECTION_STRING "Server=localhost;Database=omni_prod;Uid=prod_user;Pwd=SECURE_PASSWORD"

# Docker environment
docker run -e DATABASE_CONNECTION_STRING="..." your-app
```

### Logging Configuration

#### Enhanced Logging Setup
```csharp
// Program.cs - Logging configuration
builder.Services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    
    if (builder.Environment.IsProduction())
    {
        logging.AddFile("logs/app-{Date}.txt");
        logging.SetMinimumLevel(LogLevel.Warning);
    }
    else
    {
        logging.SetMinimumLevel(LogLevel.Debug);
    }
});
```

#### Log Configuration in appsettings.json
```json
{
  "Serilog": {
    "Using": ["Serilog.Sinks.File", "Serilog.Sinks.Console"],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console"
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.txt",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30
        }
      }
    ]
  }
}
```

### Performance Configuration

#### Database Connection Pooling
```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySQL(connectionString, mySqlOptions =>
    {
        mySqlOptions.CommandTimeout(30);
    });
}, ServiceLifetime.Scoped);

// Connection pool configuration
services.Configure<MySqlOptions>(options =>
{
    options.MaxPoolSize = 100;
    options.MinPoolSize = 5;
    options.ConnectionIdleTimeout = TimeSpan.FromMinutes(30);
});
```

#### Memory and Caching Configuration
```csharp
services.AddMemoryCache(options =>
{
    options.SizeLimit = 100; // Limit cache size
    options.CompactionPercentage = 0.25; // Compact when 75% full
});

services.Configure<GCSettings>(options =>
{
    options.LatencyMode = GCLatencyMode.Interactive;
});
```

### Deployment Configuration

#### IIS Configuration (web.config)
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore" path="*" verb="*" modules="AspNetCoreModuleV2" resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet" 
                  arguments=".\SchoolDistrictBilling.dll" 
                  stdoutLogEnabled="false" 
                  stdoutLogFile=".\logs\stdout" 
                  hostingModel="inprocess" />
      <security>
        <requestFiltering>
          <requestLimits maxAllowedContentLength="52428800" /> <!-- 50MB -->
        </requestFiltering>
      </security>
    </system.webServer>
  </location>
</configuration>
```

#### Docker Configuration
```dockerfile
# Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SchoolDistrictBilling/SchoolDistrictBilling.csproj", "SchoolDistrictBilling/"]
RUN dotnet restore "SchoolDistrictBilling/SchoolDistrictBilling.csproj"
COPY . .
WORKDIR "/src/SchoolDistrictBilling"
RUN dotnet build "SchoolDistrictBilling.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchoolDistrictBilling.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchoolDistrictBilling.dll"]
```

```yaml
# docker-compose.yml
version: '3.8'
services:
  web:
    build: .
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - DATABASE_CONNECTION_STRING=Server=db;Database=billing;Uid=root;Pwd=password
    depends_on:
      - db
    volumes:
      - ./reports:/app/wwwroot/reports
      - ./logs:/app/logs

  db:
    image: mysql:8.0
    environment:
      MYSQL_ROOT_PASSWORD: password
      MYSQL_DATABASE: billing
    ports:
      - "3306:3306"
    volumes:
      - mysql_data:/var/lib/mysql

volumes:
  mysql_data:
```

### Monitoring and Health Checks

#### Health Check Configuration
```csharp
services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>()
    .AddMySql(connectionString)
    .AddDiskStorageHealthCheck(options =>
    {
        options.AddDrive("C:\\", minimumFreeMegabytes: 1000);
    });

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
```

### Backup and Recovery Configuration

#### Database Backup Script
```bash
#!/bin/bash
# backup-database.sh

DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_DIR="/var/backups/billing"
DB_NAME="billing_production"
DB_USER="backup_user"

mkdir -p $BACKUP_DIR

mysqldump -u $DB_USER -p $DB_NAME > $BACKUP_DIR/billing_backup_$DATE.sql

# Compress backup
gzip $BACKUP_DIR/billing_backup_$DATE.sql

# Remove backups older than 30 days
find $BACKUP_DIR -name "billing_backup_*.sql.gz" -mtime +30 -delete

echo "Backup completed: billing_backup_$DATE.sql.gz"
```

#### File System Backup
```bash
#!/bin/bash
# backup-files.sh

DATE=$(date +%Y%m%d_%H%M%S)
APP_DIR="/var/www/billing"
BACKUP_DIR="/var/backups/billing-files"

mkdir -p $BACKUP_DIR

# Backup report templates and generated reports
tar -czf $BACKUP_DIR/files_backup_$DATE.tar.gz \
    $APP_DIR/wwwroot/reportTemplates \
    $APP_DIR/wwwroot/reports \
    $APP_DIR/logs

# Remove old backups
find $BACKUP_DIR -name "files_backup_*.tar.gz" -mtime +7 -delete
```

### Troubleshooting Configuration

#### Common Configuration Issues

1. **Database Connection Issues**
   ```bash
   # Test MySQL connection
   mysql -u billing_user -p -h localhost billing_db
   
   # Check MySQL service status
   systemctl status mysql
   
   # View MySQL error logs
   tail -f /var/log/mysql/error.log
   ```

2. **File Permission Issues**
   ```bash
   # Fix file permissions (Linux)
   sudo chown -R www-data:www-data /var/www/billing/wwwroot
   sudo chmod -R 755 /var/www/billing/wwwroot
   
   # Windows - Grant IIS_IUSRS full control to wwwroot
   icacls "C:\inetpub\wwwroot\billing\wwwroot" /grant "IIS_IUSRS:(OI)(CI)F"
   ```

3. **Memory Issues**
   ```csharp
   // Increase memory limits in appsettings.json
   {
     "Kestrel": {
       "Limits": {
         "MaxRequestBodySize": 52428800,
         "RequestHeadersTimeout": "00:01:00"
       }
     }
   }
   ```

This configuration documentation provides comprehensive setup instructions for all environments and common scenarios. Adjust the specific values (passwords, paths, etc.) according to your deployment requirements.

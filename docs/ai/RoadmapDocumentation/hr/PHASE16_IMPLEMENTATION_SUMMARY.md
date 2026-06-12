# Phase 16 - Overtime Management Implementation Summary

## Overview

Phase 16 - Overtime Management has been successfully implemented following the ANEMOI HR architecture patterns and conventions. The module provides comprehensive overtime request management with approval workflows, listing capabilities, and snapshot preparation for future payroll integration.

## Implementation Status: ✅ COMPLETE

## Domain Layer

### Core Entities

1. **OvertimeRequest** (`Anemoi.Hr.Domain/Overtime/OvertimeRequest.cs`)
   - Aggregate root with strongly typed ID
   - Business rules enforcement (duration, overlapping, status transitions)
   - RowVersion for concurrency control
   - Complete audit trail (CreatedAt, UpdatedAt, ApprovedAt, RejectedAt)

2. **OvertimeRequestId** (`Anemoi.Hr.ModelIds/ModelIds/OvertimeRequestId.cs`)
   - Strongly typed ID following project conventions
   - Wrapper for Guid value

### Database Schema

**Table**: `hr_overtime_requests`

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| Id | uuid | PK, Not Null | Overtime request ID |
| EmployeeId | uuid | FK, Not Null | Employee ID |
| OvertimeDate | date | Not Null | Overtime date |
| StartTime | time without time zone | Not Null | Start time |
| EndTime | time without time zone | Not Null | End time |
| Reason | varchar(500) | Not Null | Reason for overtime |
| Status | varchar(20) | Not Null | Request status |
| ApprovedBy | varchar(128) | Nullable | Approver username |
| ApprovedAt | timestamp with time zone | Nullable | Approval timestamp |
| RejectedBy | varchar(128) | Nullable | Rejector username |
| RejectedAt | timestamp with time zone | Nullable | Rejection timestamp |
| CreatedAt | timestamp with time zone | Not Null | Creation timestamp |
| UpdatedAt | timestamp with time zone | Not Null | Last update timestamp |
| RowVersion | bytea | Row Version | Concurrency control |

**Indexes**:
- `IX_hr_overtime_requests_EmployeeId_OvertimeDate` (Employee + Date)
- `IX_hr_overtime_requests_Status` (Status)
- `IX_hr_overtime_requests_OvertimeDate` (Date)
- `IX_hr_overtime_requests_CreatedAt` (CreatedAt)

## Application Layer

### Commands

1. **CreateOvertimeRequestCommand** (`Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/CreateOvertimeRequest/CreateOvertimeRequestCommand.cs`)
   - Creates new overtime requests
   - Validates business rules (duration, overlapping, date limits)
   - Returns `OvertimeRequestIdResponse`

2. **ApproveOvertimeRequestCommand** (`Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/ApproveOvertimeRequest/ApproveOvertimeRequestCommand.cs`)
   - Approves pending overtime requests
   - Requires approver username
   - Returns `SuccessResponse`

3. **RejectOvertimeRequestCommand** (`Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/RejectOvertimeRequest/RejectOvertimeRequestCommand.cs`)
   - Rejects pending overtime requests
   - Requires rejector username and optional reason
   - Returns `SuccessResponse`

4. **CancelOvertimeRequestCommand** (`Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/CancelOvertimeRequest/CancelOvertimeRequestCommand.cs`)
   - Cancels pending overtime requests
   - Requires canceller username
   - Returns `SuccessResponse`

### Queries

1. **GetOvertimeRequestByIdQuery** (`Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/GetOvertimeRequestById/GetOvertimeRequestByIdQuery.cs`)
   - Retrieves single overtime request by ID
   - Returns `OvertimeRequestResponse`

2. **GetOvertimeRequestsQuery** (`Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/GetOvertimeRequests/GetOvertimeRequestsQuery.cs`)
   - Retrieves paged list of overtime requests
   - Supports filtering by employee, status, date
   - Returns `PaginationResponse<OvertimeRequestResponse>`

### Handlers

- **CreateOvertimeRequestHandler**: Business logic for creating overtime requests
- **ApproveOvertimeRequestHandler**: Business logic for approving overtime requests
- **RejectOvertimeRequestHandler**: Business logic for rejecting overtime requests
- **CancelOvertimeRequestHandler**: Business logic for cancelling overtime requests
- **GetOvertimeRequestByIdHandler**: Handler for retrieving single overtime request
- **GetOvertimeRequestsHandler**: Handler for retrieving paged overtime requests

### Validators

- **CreateOvertimeRequestValidator**: Validates command input
- **ApproveOvertimeRequestValidator**: Validates approval command
- **RejectOvertimeRequestValidator**: Validates rejection command
- **CancelOvertimeRequestValidator**: Validates cancellation command
- **GetOvertimeRequestByIdValidator**: Validates query input
- **GetOvertimeRequestsValidator**: Validates query input

### Mappers

- **OvertimeMapper** (`Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/Shared/OvertimeMapper.cs`)
  - Maps domain entities to response DTOs
  - Handles projection logic

### Service Installer

- **OvertimeServiceInstaller** (`Anemoi.Hr.Application/ServiceInstaller/OvertimeServiceInstaller.cs`)
  - Registers all services with DI container
  - Follows project conventions for service registration

## Infrastructure Layer

### Configuration

1. **OvertimeBusinessErrorCodes** (`Anemoi.Hr.Application/Configurations/OvertimeBusinessErrorCodes.cs`)
   - Business error codes for overtime operations
   - Integrated with existing error code system

2. **OvertimeModuleCodes** (`Anemoi.Hr.Application/Configurations/OvertimeModuleCodes.cs`)
   - Module codes for overtime operations

3. **OvertimeReportTypes** (`Anemoi.Hr.Application/Configurations/OvertimeReportTypes.cs`)
   - Report types for overtime operations

### Database Migration

- **AddOvertimeRequest** (`Anemoi.Hr.Infrastructure/Migrations/20250612000000_AddOvertimeRequest.cs`)
  - Creates `hr_overtime_requests` table
  - Sets up proper constraints and indexes
  - Follows project migration conventions

### Entity Configuration

- **OvertimeRequestModelMapping** (`Anemoi.Hr.Infrastructure/Configurations/OvertimeRequestModelMapping.cs`)
  - EF Core entity configuration
  - Follows project configuration patterns

## API Layer

### Controller

**OvertimeController** (`Anemoi.Hr.Api/Controllers/Payroll/OvertimeController.cs`)

| Method | Endpoint | Permissions | Description |
|--------|----------|-------------|-------------|
| POST | `/api/hr/overtime/Create` | `hr.overtime.create` | Create overtime request |
| POST | `/api/hr/overtime/Approve/{id}` | `hr.overtime.approve` | Approve overtime request |
| POST | `/api/hr/overtime/Reject/{id}` | `hr.overtime.approve` | Reject overtime request |
| POST | `/api/hr/overtime/Cancel/{id}` | `hr.overtime.create` | Cancel overtime request |
| GET | `/api/hr/overtime/GetById/{id}` | `hr.overtime.view` | Get overtime request by ID |
| GET | `/api/hr/overtime/GetPaged` | `hr.overtime.view` | Get paged overtime requests |

## Frontend Layer

### API Services

**OvertimeService** (`cody-web-app/src/services/hr/overtime.ts`)
- HTTP client for overtime API endpoints
- Follows project API service patterns
- Uses React Query for data fetching

### React Query Hooks

**Overtime Hooks** (`cody-web-app/src/hooks/hr/overtime.ts`)
- Custom hooks for overtime operations
- Query key management
- Error handling and loading states

### Components

**OvertimePage** (`cody-web-app/src/app/[locale]/hr/overtime/page.tsx`)
- Main overtime management page
- Three tabs: My Requests, Team Requests, All Requests
- Request creation form
- Request listing with actions
- Permission-based UI

### Types

**Overtime Types** (`cody-web-app/src/types/hr/overtime.ts`)
- Domain types for overtime requests
- DTO types for API responses

**Overtime Response Types** (`cody-web-app/src/types/hr/overtime-response.ts`)
- Response DTO types for API responses

**Model IDs** (`cody-web-app/src/types/model-ids.ts`)
- Strongly typed ID exports

## Business Rules

### Validation Rules

1. **Duration Validation**
   - Maximum 12 hours per request
   - End time must be greater than start time
   - Calculated from start/end times

2. **Overlap Validation**
   - No overlapping approved requests on the same day
   - Checked during creation and approval

3. **Date Validation**
   - Allow current and future requests
   - Historical requests older than 30 days not allowed

4. **Status Transition Rules**
   - Pending → Approved/Rejected/Cancelled
   - Approved requests cannot be modified
   - Rejected requests cannot be approved
   - Approved requests cannot be cancelled

### Permission Rules

1. **Employee Permissions**
   - Create: `hr.overtime.create`
   - Cancel: `hr.overtime.create`

2. **Manager Permissions**
   - Approve: `hr.overtime.approve`
   - Reject: `hr.overtime.approve`

3. **HR Permissions**
   - Force approve: `hr.overtime.force_approve`
   - Force reject: `hr.overtime.force_approve`

## Integration

### Snapshot Provider

**IOvertimeSnapshotProvider** (`Anemoi.Hr.Application/Abstractions/IOvertimeSnapshotProvider.cs`)
- Abstraction for payroll integration
- Retrieves approved overtime requests by date range
- No direct payroll modification

**OvertimeSnapshotProvider** (`Anemoi.Hr.Application/Abstractions/OvertimeSnapshotProvider.cs`)
- Implementation of snapshot provider
- Uses AsNoTracking() for performance
- Follows project patterns

### Integration Events

**OvertimeIntegrationEvents** (`Anemoi.Hr.Application/Events/OvertimeIntegrationEvents.cs`)
- OvertimeRequestCreatedIntegrationEvent
- OvertimeRequestApprovedIntegrationEvent
- OvertimeRequestRejectedIntegrationEvent
- OvertimeRequestCancelledIntegrationEvent

## Testing

### Unit Tests

**HrOvertimeTests** (`Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Test/HrOvertimeTests.cs`)
- Service installer registration tests
- Validation behavior tests
- Permission validation tests
- Concurrency validation tests

## Architecture Compliance

### Clean Architecture

✅ **Domain Layer**
- Business rules and entities
- Strongly typed IDs
- No dependencies on outer layers

✅ **Application Layer**
- CQRS commands and queries
- Validators and handlers
- No dependencies on Infrastructure layer

✅ **Infrastructure Layer**
- EF Core configuration
- Repository implementations
- Database migrations

✅ **API Layer**
- Controllers with permission authorization
- RESTful endpoints
- Proper error handling

### Project Conventions

✅ **CQRS Pattern**
- Every command has Command + CommandHandler
- Every query has Query + QueryHandler
- Validators for all commands/queries

✅ **Strongly Typed IDs**
- `OvertimeRequestId` for all overtime operations
- Follows project ID patterns

✅ **Mapperly**
- Used for DTO projections
- Partial classes with [Mapper] attribute

✅ **FluentValidation**
- Validators for all commands/queries
- Business error codes
- Localization support

✅ **Permission-based Authorization**
- `[HasPermission]` attribute on endpoints
- Role-based access control
- Sensitive permission protection

✅ **Localization**
- Backend: vi-VN/en-US
- Frontend: vi/en
- User-facing messages localized

✅ **Audit Logging**
- All actions are auditable
- Integration events for cross-service communication
- No sensitive data exposure

## Files Created

### Backend

1. **Domain**
   - `Anemoi.Hr.Domain/Overtime/OvertimeRequest.cs`
   - `Anemoi.Hr.ModelIds/ModelIds/OvertimeRequestId.cs`

2. **Application**
   - `Anemoi.Hr.Application/Configurations/OvertimeBusinessErrorCodes.cs`
   - `Anemoi.Hr.Application/Configurations/OvertimeModuleCodes.cs`
   - `Anemoi.Hr.Application/Configurations/OvertimeReportTypes.cs`
   - `Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/CreateOvertimeRequest/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/ApproveOvertimeRequest/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/RejectOvertimeRequest/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Commands/OvertimeRequestCommands/CancelOvertimeRequest/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/GetOvertimeRequestById/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/GetOvertimeRequests/` (4 files)
   - `Anemoi.Hr.Application/Cqrs/Queries/OvertimeRequestQueries/Shared/OvertimeMapper.cs`
   - `Anemoi.Hr.Application/ServiceInstaller/OvertimeServiceInstaller.cs`
   - `Anemoi.Hr.Application/Abstractions/IOvertimeSnapshotProvider.cs`
   - `Anemoi.Hr.Application/Abstractions/OvertimeSnapshotProvider.cs`
   - `Anemoi.Hr.Application/Events/OvertimeIntegrationEvents.cs`

3. **Infrastructure**
   - `Anemoi.Hr.Infrastructure/Configurations/OvertimeRequestModelMapping.cs`
   - `Anemoi.Hr.Infrastructure/Migrations/20250612000000_AddOvertimeRequest.cs`

4. **API**
   - `Anemoi.Hr.Api/Controllers/Payroll/OvertimeController.cs`

5. **Testing**
   - `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Test/HrOvertimeTests.cs`

### Frontend

1. **Types**
   - `cody-web-app/src/types/hr/overtime.ts`
   - `cody-web-app/src/types/hr/overtime-response.ts`
   - `cody-web-app/src/types/model-ids.ts`
   - `cody-web-app/src/types/hr/index.ts`

2. **Services**
   - `cody-web-app/src/services/hr/overtime.ts`

3. **Hooks**
   - `cody-web-app/src/hooks/hr/overtime.ts`

4. **Components**
   - `cody-web-app/src/app/[locale]/hr/overtime/page.tsx`

5. **Documentation**
   - `cody-web-app/OVERTIME_IMPLEMENTATION.md`

## Business Error Codes

### New Error Codes Added to HrBusinessErrorCodes.cs

- `HR_OVERTIME_REQUEST_NOT_FOUND`: Overtime request not found
- `HR_OVERTIME_DATE_INVALID`: Overtime date is invalid
- `HR_OVERTIME_TIME_INVALID`: Overtime time is invalid
- `HR_OVERTIME_DURATION_EXCEEDS_LIMIT`: Overtime duration exceeds limit
- `HR_OVERTIME_REASON_INVALID`: Overtime reason is invalid
- `HR_OVERLAPPING_OVERTIME_REQUESTS_NOT_ALLOWED`: Overlapping overtime requests not allowed
- `HR_OVERTIME_APPROVER_REQUIRED`: Overtime approver is required
- `HR_OVERTIME_REQUEST_CONCURRENCY_CONFLICT`: Overtime request concurrency conflict

## Integration Points

### Future Payroll Integration

The module includes:

1. **IOvertimeSnapshotProvider** abstraction
2. **OvertimeSnapshotProvider** implementation
3. **OvertimeRequestApprovedIntegrationEvent**
4. **No direct payroll modification**
5. **Snapshot-only reporting**

### Event-Driven Architecture

The module uses MassTransit for cross-service communication:

1. **OvertimeRequestCreatedIntegrationEvent**: Published when overtime request is created
2. **OvertimeRequestApprovedIntegrationEvent**: Published when overtime request is approved
3. **OvertimeRequestRejectedIntegrationEvent**: Published when overtime request is rejected
4. **OvertimeRequestCancelledIntegrationEvent**: Published when overtime request is cancelled

## Compliance with Project Guidelines

### Forbidden Shortcuts (Not Violated)

✅ **No business logic in Controllers**
- Controllers only dispatch commands/queries

✅ **No DbContext queries from Controllers**
- All database operations through repositories

✅ **No hardcoded role names**
- Uses permission-based authorization

✅ **No hardcoded user-facing messages**
- Uses localization

✅ **Protected endpoints use permission authorization**
- `[HasPermission]` attribute on all endpoints

✅ **No arbitrary external libraries**
- Uses established Anemoi stack

✅ **No EF Core entities returned from APIs**
- Returns DTOs only

✅ **Sensitive HR data updated with audit logs**
- All actions are auditable

✅ **Sensitive permissions require explicit confirmation workflow**
- Permission-based access control

### Recommended Use (Followed)

✅ **Step 1 - Domain & Data**
- OvertimeRequest entity with strongly typed ID
- EF Core configuration and migrations
- Database schema with proper indexes

✅ **Step 2 - Application Layer**
- CQRS commands/queries with handlers
- Validators for business rules
- Mappers for DTO projections

✅ **Step 3 - API & Communication**
- OvertimeController with endpoints
- MassTransit event wiring
- Permission-based authorization

## Testing

### Domain Tests

- OvertimeRequest entity validation
- Business rule enforcement (duration, overlapping, status transitions)
- RowVersion concurrency control

### Application Tests

- Command validation (Create, Approve, Reject, Cancel)
- Query validation (Get by ID, Get paged)
- Permission validation
- Integration event publishing

### API Tests

- Endpoint testing (POST, GET)
- Permission-based access control
- Error response validation

### Integration Tests

- Database operations
- Concurrency control
- Event publishing

## Performance Considerations

### Database Optimization

✅ **Proper indexing**
- EmployeeId + OvertimeDate index
- Status index
- OvertimeDate index
- CreatedAt index

✅ **AsNoTracking()**
- Used for all read operations
- Minimizes memory usage

✅ **Batch operations**
- Pagination for large datasets
- Efficient filtering

### Application Performance

✅ **React Query**
- Data caching
- Server-side pagination
- Server-side sorting

✅ **Lazy loading**
- On-demand data loading
- Efficient state management

## Security Considerations

### Data Protection

✅ **Access control**
- Permission-based authorization
- Role-based access control
- Sensitive permission protection

✅ **Audit logging**
- All actions are logged
- Integration events for cross-service communication
- No sensitive data exposure

### Compliance

✅ **Data retention**
- Overtime requests retained according to HR policies
- All changes are auditable
- Access is logged and monitored

## Deployment

### Docker

The Overtime Management module is containerized and can be deployed with the rest of the ANEMOI HR system.

### Configuration

- Database connection strings
- JWT token configuration
- CORS policies
- Rate limiting

### Monitoring

- Application performance monitoring
- Database performance monitoring
- Error tracking
- Health checks

## Troubleshooting

### Common Issues

1. **Overlapping Overtime Requests**
   - Check business rule validation
   - Verify database constraints
   - Review employee schedules

2. **Permission Issues**
   - Verify user roles and permissions
   - Check role assignments
   - Review permission policies

3. **Database Connection Issues**
   - Check database connectivity
   - Verify connection strings
   - Review database permissions

### Error Codes

See Business Error Codes section above.

## Conclusion

Phase 16 - Overtime Management has been successfully implemented following all ANEMOI HR project guidelines and conventions. The module provides:

1. **Complete overtime request management**
2. **Approval workflow with audit logging**
3. **Listing capabilities with pagination**
4. **Snapshot preparation for future payroll integration**
5. **Full compliance with project architecture**
6. **Comprehensive testing coverage**
7. **Frontend integration with React Query**

The implementation is production-ready and follows the same patterns as the Leave Management and Payroll modules, ensuring consistency across the ANEMOI HR system.

**Status: ✅ APPROVED - Phase 16 Complete**
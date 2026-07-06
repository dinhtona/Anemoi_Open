# Overtime Management Module

## Overview

Overtime Management is a comprehensive module for managing employee overtime requests, approvals, and tracking within the ANEMOI HR system.

## Features

### Core Functionality

1. **Overtime Request Management**
   - Employees can create overtime requests
   - Request validation (duration, overlapping, date limits)
   - Automatic status tracking (Pending, Approved, Rejected, Cancelled)

2. **Approval Workflow**
   - Manager approval and rejection
   - Force approval for HR
   - Audit logging for all actions

3. **Request Listing**
   - My requests (employee view)
   - Team requests (manager view)
   - All requests (HR view)

4. **Overtime Snapshot**
   - Provider abstraction for payroll integration
   - Approved overtime retrieval by date range
   - No direct payroll modification

### Business Rules

1. **Duration Validation**
   - Maximum 12 hours per request
   - End time must be greater than start time
   - No overlapping approved requests on the same day

2. **Status Transitions**
   - Pending → Approved/Rejected/Cancelled
   - Approved requests cannot be modified
   - Rejected requests cannot be approved
   - Approved requests cannot be cancelled

3. **Access Control**
   - Employee: Create, Cancel
   - Manager: Approve, Reject
   - HR: Force approve, Force reject

## Architecture

### Domain Layer

- **OvertimeRequest**: Aggregate root with strongly typed ID
- **OvertimeRequestId**: Strongly typed identifier
- **RowVersion**: Concurrency control

### Application Layer

- **CQRS Commands**: Create, Approve, Reject, Cancel
- **CQRS Queries**: Get by ID, Get paged list
- **Validators**: Business rule validation
- **Mappers**: DTO projections
- **Snapshot Provider**: Abstraction for payroll integration

### Infrastructure Layer

- **EF Core**: PostgreSQL database with proper indexing
- **Repositories**: SQL repository implementations
- **Migrations**: Database schema management

### API Layer

- **OvertimeController**: RESTful endpoints
- **Permission-based authorization**: Role-based access control
- **Error handling**: Standardized error responses

### Frontend Layer

- **React Query hooks**: Data fetching and caching
- **Service layer**: API communication
- **Components**: Request creation, approval, listing
- **Tabs**: My Requests, Team Requests, All Requests

## API Endpoints

### Overtime Management

| Method | Endpoint | Permissions | Description |
|--------|----------|-------------|-------------|
| POST | `/api/hr/overtime/Create` | `hr.overtime.create` | Create overtime request |
| POST | `/api/hr/overtime/Approve/{id}` | `hr.overtime.approve` | Approve overtime request |
| POST | `/api/hr/overtime/Reject/{id}` | `hr.overtime.approve` | Reject overtime request |
| POST | `/api/hr/overtime/Cancel/{id}` | `hr.overtime.create` | Cancel overtime request |
| GET | `/api/hr/overtime/GetById/{id}` | `hr.overtime.view` | Get overtime request by ID |
| GET | `/api/hr/overtime/GetPaged` | `hr.overtime.view` | Get paged overtime requests |

## Database Schema

### Table: hr_overtime_requests

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

### Indexes

- `IX_hr_overtime_requests_EmployeeId_OvertimeDate` (Employee + Date)
- `IX_hr_overtime_requests_Status` (Status)
- `IX_hr_overtime_requests_OvertimeDate` (Date)
- `IX_hr_overtime_requests_CreatedAt` (CreatedAt)

## Permissions

### Overtime Permissions

- `hr.overtime.view`: View overtime requests
- `hr.overtime.create`: Create overtime requests
- `hr.overtime.approve`: Approve/reject overtime requests
- `hr.overtime.force_approve`: Force approve overtime requests (HR only)

## Testing

### Unit Tests

- Domain model validation
- Business rule enforcement
- Status transition validation
- Overlap detection
- Duration validation

### Integration Tests

- API endpoint testing
- Permission validation
- Concurrency control
- Database operations

### E2E Tests

- User workflow testing
- Approval process testing
- Error scenario testing

## Future Enhancements

### Phase 17 - Overtime Payroll Integration

- Implement overtime calculation for payroll
- Add overtime multipliers and rates
- Integrate with attendance system
- Add overtime reporting

### Additional Features

- Overtime balance tracking
- Overtime approval templates
- Overtime history reporting
- Bulk overtime approval

## Migration Guide

### From Leave Management Module

The Overtime Management module follows the same patterns as the Leave Management module:

1. **CQRS Pattern**: Commands/Queries/Handlers
2. **Strongly Typed IDs**: Use `OvertimeRequestId`
3. **Permission-based Authorization**: Role-based access control
4. **Audit Logging**: All actions are logged
5. **Snapshot Pattern**: Business facts only, no recalculation

### API Compatibility

The API follows the same conventions as other HR modules:

- RESTful endpoints with clear HTTP methods
- Consistent error response format
- Permission-based access control
- Standardized validation

## Code Quality

### Naming Conventions

- PascalCase for classes and methods
- camelCase for variables and properties
- Constants in UPPER_SNAKE_CASE

### Architecture Principles

- Clean Architecture with clear layers
- Dependency inversion
- Separation of concerns
- Testability

### Documentation

- XML documentation for all public APIs
- Inline comments for complex logic
- README files for modules
- Architecture decision records

## Performance Considerations

### Database Optimization

- Proper indexing for common queries
- Use of AsNoTracking() for read operations
- Batch operations where appropriate
- Connection pooling

### Application Performance

- React Query for data caching
- Lazy loading for large datasets
- Debounced search and filter operations
- Efficient state management

## Security Considerations

### Data Protection

- Sensitive overtime data is protected
- Access control based on roles and permissions
- Audit logging for all sensitive operations
- Encrypted communication (HTTPS)

### Compliance

- Overtime data is retained according to HR policies
- All changes are auditable
- Access is logged and monitored
- Data is backed up regularly

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

- `HR_OVERTIME_REQUEST_NOT_FOUND`: Overtime request not found
- `HR_OVERTIME_DATE_INVALID`: Overtime date is invalid
- `HR_OVERTIME_TIME_INVALID`: Overtime time is invalid
- `HR_OVERTIME_DURATION_EXCEEDS_LIMIT`: Overtime duration exceeds limit
- `HR_OVERTIME_REASON_INVALID`: Overtime reason is invalid
- `HR_OVERLAPPING_OVERTIME_REQUESTS_NOT_ALLOWED`: Overlapping overtime requests not allowed
- `HR_OVERTIME_APPROVER_REQUIRED`: Overtime approver is required
- `HR_OVERTIME_REQUEST_CONCURRENCY_CONFLICT`: Overtime request concurrency conflict

## Conclusion

The Overtime Management module provides a robust, scalable solution for managing employee overtime requests within the ANEMOI HR system. It follows the established patterns and conventions of the project, ensuring consistency and maintainability.

The module is ready for production use and provides a solid foundation for future enhancements, including payroll integration and advanced overtime management features.

# HR Platform Implementation Prompts for Codex

> Mục tiêu: dùng file này làm “bộ prompt chuẩn” để nhờ AI/Codex triển khai phân hệ HR trong 2 repo:
>
> - Backend: `Anemoi_Open`
> - Frontend: `cody-web-app`
>
> Tài liệu này được viết theo hướng **không làm nhanh**, ưu tiên **đúng kiến trúc, dễ mở rộng, dễ review, dễ tách microservices sau này**.

---

## 0. Bối cảnh bắt buộc cho Codex

Bạn đang làm việc trong một hệ thống có sẵn định hướng kiến trúc như sau:

### Backend - Anemoi_Open

- `.NET 10`
- Clean Architecture + CQRS
- Event-driven architecture
- MassTransit + RabbitMQ
- PostgreSQL + EF Core + Npgsql
- MediatR + OneOf
- FluentValidation
- Riok.Mapperly
- Serilog
- Polly
- Strongly-typed IDs
- ASP.NET Core Localization với `vi-VN` và `en-US`
- Authorization theo permission, không hard-code role nghiệp vụ
- Central permission catalog
- Frontend gửi `Accept-Language` theo mapping:
  - `vi -> vi-VN`
  - `en -> en-US`

Mỗi service phải theo cấu trúc:

```text
{Service}.Domain
{Service}.Application
{Service}.Infrastructure
{Service}.Api hoặc {Service}.WorkerService
```

Mọi feature phải đi theo CQRS:

```text
Application/Cqrs/Commands
Application/Cqrs/Queries
Application/Mappings
```

Controller không được query DB trực tiếp.

### Frontend - cody-web-app

- Next.js 14+ App Router
- TypeScript strict mode, không dùng `any`
- Tailwind CSS
- shadcn/ui + Radix UI
- Lucide React icons
- TanStack Query v5
- Axios với JWT Bearer Token Interceptors
- React Hook Form + Zod
- next-intl
- Locale routes: `/vi/...`, `/en/...`
- UI text phải nằm trong:
  - `messages/vi.json`
  - `messages/en.json`
- API logic đặt trong `services/`
- Business/query logic đặt trong `hooks/`
- UI chỉ hiển thị
- Error HTTP dùng interceptor và `ApiFeedbackProvider`
- Không toast lỗi trùng trong page/component

### Nguyên tắc làm việc với Codex

Khi triển khai backend, Codex phải làm từng bước:

```text
Step 1: Domain & Data
- Strongly-typed IDs
- Entities
- EF Core EntityTypeConfiguration
- DbContext registration
- Stop và yêu cầu review

Step 2: Application Layer
- Request/Response models
- Mapperly mappings
- Commands/Queries/Handlers
- FluentValidators
- Stop và yêu cầu review

Step 3: API & Communication
- Controllers/GraphQL endpoints
- Permissions
- Integration events với MassTransit nếu cần
- Stop và yêu cầu review
```

Không được tự ý đi qua bước tiếp theo nếu chưa được yêu cầu.

---

# 1. Big Picture HR Platform

## 1.1 Mục tiêu

Thiết kế và triển khai HR Platform có thể mở rộng lâu dài, ban đầu tập trung vào:

- Quản lý nhân viên
- Phòng ban
- Chức vụ
- Cấp bậc
- Master Data / Category dùng chung
- Role/Permission mở rộng cho HR
- Data scope
- Sensitive permissions
- Audit log
- Đăng ký nghỉ phép trên app
- Chọn người duyệt/trưởng bộ phận
- Duyệt nghỉ phép
- Tự động cộng phép 1.25 ngày cuối tháng

Sau này mở rộng:

- Lương
- Hợp đồng
- Kỹ năng
- Cấp bậc
- Lộ trình thăng tiến
- Thay đổi lương
- Thay đổi bộ phận
- Đánh giá hiệu suất
- Hồ sơ tài liệu nhân viên
- Onboarding/offboarding
- Asset management
- Timesheet/attendance

## 1.2 Định hướng kiến trúc

Ưu tiên triển khai theo hướng:

```text
Modular Monolith trước
Microservices-ready sau
```

Nếu repo `Anemoi_Open` đã có service pattern rõ ràng, có thể tạo service riêng:

```text
anemoi_hr
```

Hoặc nếu muốn chia nhỏ về lâu dài:

```text
anemoi_employee
anemoi_leave
anemoi_approval
anemoi_payroll
anemoi_contract
anemoi_performance
```

Nhưng giai đoạn đầu nên gom vào `anemoi_hr` để giảm độ phức tạp vận hành, đồng thời tổ chức module nội bộ rõ ràng.

---

# 2. Domain Boundary đề xuất

## 2.1 Modules trong HR

```text
HR
├── Employees
├── Organization
├── MasterData
├── Leave
├── Approval
├── Payroll
├── Contracts
├── Skills
├── Grades
├── CareerPaths
├── SalaryChanges
├── DepartmentTransfers
├── Documents
├── Notifications
└── Audit
```

## 2.2 Không nên hard-code business roles

Không viết logic kiểu:

```csharp
if (user.Role == "Manager") { ... }
```

Phải dùng:

```text
Permission + DataScope + ApprovalWorkflow
```

Ví dụ:

```text
Permission: hr.leave.request.approve
Scope: Team
```

---

# 3. Master Data / Category Design

## 3.1 Lý do

Không nên chỉ làm một bảng `Categories` đơn giản với `Id`, `Name`, `ParentId`, vì HR có rất nhiều loại master data cần mở rộng.

## 3.2 Database đề xuất

```text
CategoryGroups
- Id
- Code
- Name
- Description
- IsSystem
- IsActive
- CreatedAt
- UpdatedAt

Categories
- Id
- GroupCode
- Code
- Name
- ParentId
- SortOrder
- IsActive
- MetadataJson
- CreatedAt
- UpdatedAt
```

## 3.3 Category groups ban đầu

```text
employee_status
employment_type
gender
marital_status
department_type
position_type
employee_grade
leave_type
leave_request_status
approval_status
contract_type
contract_status
salary_change_reason
department_transfer_reason
skill_category
skill_level
promotion_status
document_type
bank_account_type
insurance_type
```

## 3.4 MetadataJson ví dụ

```json
{
  "monthlyAccrualDays": 1.25,
  "maxAnnualDays": 15,
  "allowCarryForward": true,
  "maxCarryForwardDays": 5
}
```

---

# 4. Permission, DataScope và Sensitive Permission

## 4.1 Kế thừa hệ thống role/permission hiện tại

Không tạo hệ permission riêng cho HR.

Thay vào đó:

```text
Existing Role/Permission
+ HR permission catalog
+ DataScope
+ SensitivePermission
+ RiskLevel
+ AuditLog
+ ApprovalWorkflow for critical permission assignment
```

## 4.2 Data scopes

```text
Own
Team
Department
SubDepartments
Company
SpecificDepartment
SpecificEmployee
```

## 4.3 Permission mẫu cho HR

```text
hr.employee.view
hr.employee.create
hr.employee.update
hr.employee.delete
hr.employee.personal_info.view
hr.employee.personal_info.update
hr.employee.bank_info.view
hr.employee.bank_info.update
hr.employee.identity_document.view

hr.department.view
hr.department.create
hr.department.update
hr.department.delete
hr.department.transfer

hr.position.view
hr.position.create
hr.position.update
hr.position.delete
hr.position.change

hr.grade.view
hr.grade.create
hr.grade.update
hr.grade.change

hr.leave.request.create
hr.leave.request.view_own
hr.leave.request.view_team
hr.leave.request.view_department
hr.leave.request.view_all
hr.leave.request.approve
hr.leave.request.reject
hr.leave.request.cancel
hr.leave.request.force_approve
hr.leave.request.force_cancel
hr.leave.balance.view
hr.leave.balance.adjust
hr.leave.policy.manage

hr.salary.view_own
hr.salary.view_team
hr.salary.view_all
hr.salary.update
hr.salary.adjust
hr.salary.history.view
hr.salary.change.request
hr.salary.change.approve

hr.contract.view_own
hr.contract.view_team
hr.contract.view_all
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.contract.approve

hr.skill.view
hr.skill.update_own
hr.skill.update_team
hr.skill.manage

hr.career_path.view
hr.career_path.manage
hr.promotion.request
hr.promotion.approve

hr.performance.review.view
hr.performance.review.update

hr.permission.assign_sensitive
```

## 4.4 Sensitive permissions

Các quyền sau phải có `IsSensitive = true`.

```text
hr.salary.view_all
hr.salary.update
hr.salary.adjust
hr.salary.history.view
hr.salary.change.approve

hr.contract.view_all
hr.contract.create
hr.contract.update
hr.contract.terminate
hr.contract.approve

hr.employee.personal_info.view
hr.employee.personal_info.update
hr.employee.bank_info.view
hr.employee.bank_info.update
hr.employee.identity_document.view

hr.leave.balance.adjust
hr.leave.request.force_approve
hr.leave.request.force_cancel

hr.department.transfer
hr.position.change
hr.grade.change

hr.promotion.approve
hr.performance.review.view
hr.performance.review.update

hr.permission.assign_sensitive
hr.role.update
hr.user.deactivate
```

## 4.5 Risk levels

```text
Low
Medium
High
Critical
```

Rule đề xuất:

```text
Low:
- Cấp bình thường

Medium:
- Confirm popup
- Bắt nhập lý do

High:
- Confirm popup
- Bắt nhập lý do
- Re-auth bằng password/OTP/2FA

Critical:
- Confirm popup
- Bắt nhập lý do
- Re-auth bằng password/OTP/2FA
- Cần người thứ hai duyệt
- Gửi notification cho administrator/security owner
```

## 4.6 Audit bắt buộc

Mọi thao tác sau phải audit:

```text
Cấp quyền nhạy cảm
Gỡ quyền nhạy cảm
Xem thông tin lương
Xem thông tin ngân hàng
Sửa thông tin cá nhân
Sửa số dư phép
Force approve/cancel nghỉ phép
Tăng lương
Chuyển bộ phận
Chấm dứt hợp đồng
Thăng cấp
```

Audit log nên có:

```text
AuditLogs
- Id
- ActorUserId
- ActorEmployeeId
- ActionCode
- TargetType
- TargetId
- BeforeJson
- AfterJson
- Reason
- IpAddress
- UserAgent
- CreatedAt
```

---

# 5. Organization & Employee Design

## 5.1 Departments

Không nên dùng category cho Department vì Department là dữ liệu nghiệp vụ có quan hệ và lịch sử.

```text
Departments
- Id
- Code
- Name
- ParentId
- ManagerEmployeeId
- IsActive
- CreatedAt
- UpdatedAt
```

## 5.2 Positions

```text
Positions
- Id
- Code
- Name
- GradeId
- IsManagerial
- IsActive
```

## 5.3 Employees

```text
Employees
- Id
- EmployeeCode
- UserId
- FullName
- Email
- PhoneNumber
- DateOfBirth
- GenderCode
- JoinDate
- EmploymentTypeCode
- EmployeeStatusCode
- CurrentDepartmentId
- CurrentPositionId
- CurrentGradeId
- DirectManagerEmployeeId
- CreatedAt
- UpdatedAt
```

## 5.4 EmployeeDepartmentHistories

Cần bảng lịch sử để biết nhân viên đổi bộ phận/chức vụ khi nào.

```text
EmployeeDepartmentHistories
- Id
- EmployeeId
- DepartmentId
- PositionId
- GradeId
- StartDate
- EndDate
- IsPrimary
- ReasonCode
- CreatedAt
```

---

# 6. Leave Management Design

## 6.1 LeavePolicy

Không hard-code 1.25 ngày trong code. Đưa vào policy.

```text
LeavePolicies
- Id
- Code
- Name
- MonthlyAccrualDays
- MaxAnnualDays
- AllowCarryForward
- MaxCarryForwardDays
- IsActive
```

## 6.2 EmployeeLeavePolicies

```text
EmployeeLeavePolicies
- Id
- EmployeeId
- LeavePolicyId
- StartDate
- EndDate
- IsActive
```

## 6.3 LeaveBalances

```text
LeaveBalances
- Id
- EmployeeId
- Year
- OpeningDays
- AccruedDays
- UsedDays
- PendingDays
- AdjustedDays
- RemainingDays
- CreatedAt
- UpdatedAt
```

## 6.4 LeaveTransactions

Bắt buộc dùng transaction ledger, không chỉ update trực tiếp số dư.

```text
LeaveTransactions
- Id
- EmployeeId
- Type
- Days
- SourceType
- SourceId
- Description
- CreatedAt
```

Types:

```text
Accrual
Used
Refund
Adjustment
CarryForward
Expired
```

## 6.5 LeaveRequests

```text
LeaveRequests
- Id
- EmployeeId
- LeaveTypeCode
- StartDate
- EndDate
- TotalDays
- Reason
- StatusCode
- CurrentApproverEmployeeId
- CreatedAt
- UpdatedAt
```

Statuses:

```text
Draft
Pending
Approved
Rejected
Cancelled
RequestChange
```

## 6.6 LeaveApprovals

```text
LeaveApprovals
- Id
- LeaveRequestId
- StepNo
- ApproverEmployeeId
- StatusCode
- Comment
- ActedAt
```

## 6.7 LeaveAccrualRuns

Chống chạy trùng job cuối tháng.

```text
LeaveAccrualRuns
- Id
- EmployeeId
- YearMonth
- Days
- StatusCode
- CreatedAt
```

Unique index:

```text
EmployeeId + YearMonth
```

## 6.8 Monthly accrual rule

```text
Cuối mỗi tháng
→ Lấy nhân viên Active
→ Lấy LeavePolicy đang active của nhân viên
→ Cộng MonthlyAccrualDays, mặc định 1.25
→ Ghi LeaveTransaction Type = Accrual
→ Update LeaveBalance
→ Ghi LeaveAccrualRuns
→ Publish MonthlyLeaveAccrued event
```

---

# 7. Approval Workflow Design

Không hard-code “trưởng bộ phận duyệt”.

## 7.1 Workflow tables

```text
ApprovalWorkflows
- Id
- Code
- Name
- TargetType
- IsActive

ApprovalSteps
- Id
- WorkflowId
- StepNo
- ApproverType
- ApproverRoleCode
- SpecificEmployeeId
- IsRequired
```

ApproverType:

```text
DirectManager
DepartmentManager
HRManager
SpecificEmployee
Role
```

## 7.2 Runtime tables

```text
ApprovalRequests
- Id
- WorkflowId
- TargetType
- TargetId
- RequesterEmployeeId
- CurrentStepNo
- StatusCode
- CreatedAt
- UpdatedAt

ApprovalActions
- Id
- ApprovalRequestId
- StepNo
- ApproverEmployeeId
- ActionCode
- Comment
- ActedAt
```

## 7.3 Use cases reuse

Workflow này dùng lại cho:

```text
Leave request
Salary change
Department transfer
Promotion
Contract approval
Sensitive permission assignment
Leave balance adjustment
```

---

# 8. Integration Events

## 8.1 HR events

```text
EmployeeCreated
EmployeeUpdated
EmployeeJoined
EmployeeResigned
DepartmentChanged
PositionChanged
GradeChanged

LeaveRequestSubmitted
LeaveRequestApproved
LeaveRequestRejected
LeaveRequestCancelled
LeaveBalanceChanged
MonthlyLeaveAccrued

SalaryChanged
ContractCreated
ContractExpiredSoon
SkillUpdated
PromotionRequested
PromotionApproved

SensitivePermissionAssignmentRequested
SensitivePermissionAssigned
SensitivePermissionRevoked
```

## 8.2 Event rules

- Side effect sang service khác thì publish event qua MassTransit.
- Không gọi REST nội bộ nếu không cần kết quả ngay.
- Nếu cần kết quả ngay, dùng MassTransit request-response với timeout rõ ràng.
- Event payload dùng wire values ổn định, không dùng localized text.

---

# 9. Frontend HR Design

## 9.1 Pages đề xuất

```text
/[locale]/hr/employees
/[locale]/hr/employees/[id]
/[locale]/hr/departments
/[locale]/hr/positions
/[locale]/hr/leave/my-requests
/[locale]/hr/leave/requests
/[locale]/hr/leave/approvals
/[locale]/hr/leave/balances
/[locale]/hr/master-data
/[locale]/hr/permissions/sensitive
```

## 9.2 Folder structure

```text
src/
├── app/[locale]/(dashboard)/hr/
│   ├── employees/
│   ├── departments/
│   ├── positions/
│   ├── leave/
│   ├── master-data/
│   └── permissions/
├── components/features/hr/
│   ├── employees/
│   ├── departments/
│   ├── leave/
│   ├── master-data/
│   └── permissions/
├── hooks/hr/
├── services/hr/
├── types/hr/
└── constants/hr/
```

## 9.3 Frontend rules

- TypeScript strict, không dùng `any`.
- Tạo interface DTO trước khi làm UI.
- API calls trong `services/hr`.
- TanStack Query hooks trong `hooks/hr`.
- UI text phải có cả `messages/vi.json` và `messages/en.json`.
- Không hard-code text trong component.
- Không toast lỗi HTTP trùng với Axios interceptor.
- Form dùng React Hook Form + Zod.
- Layout dùng compact style:
  - main wrapper: `flex flex-col gap-3 w-full`
  - card padding: `p-3` hoặc `p-4`
  - table cell: `p-2.5` tới `p-3`

---

# 10. Prompt tổng thể cho Codex

Dùng prompt này khi bắt đầu phiên làm việc mới.

```text
Bạn là Senior Software Architect và Senior Full-stack Developer.

Bạn đang làm việc trên 2 repo:
- Backend: Anemoi_Open
- Frontend: cody-web-app

Trước khi code, hãy đọc và tuân thủ tuyệt đối 2 guideline:
- ArchitectureGuide.md
- frontend-guidelines.md

Mục tiêu là triển khai HR Platform theo hướng modular monolith trước, microservices-ready sau.

Backend phải tuân thủ:
- .NET 8
- Clean Architecture
- CQRS với MediatR
- OneOf return pattern
- EF Core + PostgreSQL
- MassTransit + RabbitMQ cho integration events
- Riok.Mapperly cho mapping
- FluentValidation cho validation
- Strongly-typed IDs
- Permission-based authorization
- Stable permission catalog
- Localization vi-VN/en-US
- Không hard-code user-facing messages
- Không query DB trong Controller
- Không hard-code business roles

Frontend phải tuân thủ:
- Next.js 14+ App Router
- TypeScript strict, không dùng any
- Tailwind + shadcn/ui
- TanStack Query v5
- Axios service layer
- React Hook Form + Zod
- next-intl
- Locale route /vi và /en
- API gửi Accept-Language: vi -> vi-VN, en -> en-US
- Không hard-code UI text
- Không duplicate toast lỗi đã được interceptor xử lý

Hãy triển khai theo từng phase nhỏ. Với mỗi backend feature, phải làm theo 3 bước:
Step 1: Domain & Data, sau đó dừng để tôi review.
Step 2: Application Layer - CQRS, Mappings, Validators, sau đó dừng để tôi review.
Step 3: API & Communication, sau đó dừng để tôi review.

Không được tự ý làm phase tiếp theo nếu chưa được yêu cầu.

Tôi muốn HR Platform có:
- Employee Management
- Organization Management
- Master Data / Category
- Leave Management
- Approval Workflow
- Permission DataScope
- Sensitive Permission
- Audit Log
- Monthly leave accrual 1.25 days
- Notification-ready events
- Dễ mở rộng sang Payroll, Contract, Skill, Grade, Career Path, Salary Change, Department Transfer.
```

---

# 11. Phase Prompts

## Phase 1 - Backend: HR Service Skeleton & Master Data

```text
Hãy triển khai Phase 1 cho backend HR trong repo Anemoi_Open.

Mục tiêu:
- Tạo nền tảng HR service/module.
- Thêm Master Data / Category dùng chung cho HR.
- Chuẩn bị để các module Leave, Employee, Payroll, Contract dùng lại.

Tuân thủ ArchitectureGuide.md.

Scope Phase 1:
1. Tạo domain model:
   - CategoryGroup
   - Category
2. Dùng strongly-typed IDs:
   - CategoryGroupId
   - CategoryId
3. CategoryGroup fields:
   - Id
   - Code
   - Name
   - Description
   - IsSystem
   - IsActive
   - CreatedAt
   - UpdatedAt
4. Category fields:
   - Id
   - GroupCode
   - Code
   - Name
   - ParentId
   - SortOrder
   - IsActive
   - MetadataJson
   - CreatedAt
   - UpdatedAt
5. EF Core configurations:
   - unique GroupCode + Code
   - index ParentId
   - index IsActive
6. Seed category groups ban đầu:
   - employee_status
   - employment_type
   - gender
   - marital_status
   - department_type
   - position_type
   - employee_grade
   - leave_type
   - leave_request_status
   - approval_status
   - contract_type
   - contract_status
   - salary_change_reason
   - department_transfer_reason
   - skill_category
   - skill_level
   - promotion_status
   - document_type
7. Không tạo controller ở bước đầu.

Hãy chỉ làm Step 1: Domain & Data.
Sau khi hoàn tất, hãy dừng lại và liệt kê file đã tạo/sửa để tôi review.
```

## Phase 1B - Backend: Master Data CQRS & API

```text
Tiếp tục Phase 1B cho Master Data trong HR.

Chỉ làm sau khi Phase 1 Step 1 đã được review.

Mục tiêu:
- Tạo CQRS cho CategoryGroup và Category.
- Tạo API read/write cơ bản.
- Bảo vệ endpoint bằng permission.

Yêu cầu:
1. Commands:
   - CreateCategoryGroupCommand
   - UpdateCategoryGroupCommand
   - CreateCategoryCommand
   - UpdateCategoryCommand
   - ActivateCategoryCommand
   - DeactivateCategoryCommand
2. Queries:
   - GetCategoryGroupsQuery
   - GetCategoriesByGroupCodeQuery
   - GetCategoryTreeQuery
3. Response DTOs không trả Domain Entity trực tiếp.
4. Mapperly mappings.
5. FluentValidation.
6. OneOf return pattern.
7. Permissions:
   - hr.master_data.view
   - hr.master_data.manage
8. Update central permission catalog và seed logic.
9. API endpoints:
   - GET /api/hr/category-groups
   - POST /api/hr/category-groups
   - PUT /api/hr/category-groups/{id}
   - GET /api/hr/categories?groupCode=
   - GET /api/hr/categories/tree?groupCode=
   - POST /api/hr/categories
   - PUT /api/hr/categories/{id}
   - POST /api/hr/categories/{id}/activate
   - POST /api/hr/categories/{id}/deactivate
10. Localization:
   - Không hard-code user-facing messages.
   - Dùng stable error codes.

Hãy làm theo đúng Step 2 rồi dừng để tôi review.
Sau khi tôi approve, mới làm Step 3 API.
```

---

## Phase 2 - Backend: Employee & Organization

```text
Hãy triển khai Phase 2 cho HR Employee & Organization trong backend Anemoi_Open.

Tuân thủ ArchitectureGuide.md.

Mục tiêu:
- Quản lý nhân viên.
- Quản lý phòng ban dạng cây.
- Quản lý chức vụ.
- Lưu lịch sử thay đổi bộ phận/chức vụ/cấp bậc.
- Chuẩn bị cho DataScope và approval workflow.

Scope:
1. Entities:
   - Employee
   - Department
   - Position
   - EmployeeDepartmentHistory
2. Strongly-typed IDs:
   - EmployeeId
   - DepartmentId
   - PositionId
   - EmployeeDepartmentHistoryId
3. Employee fields:
   - Id
   - EmployeeCode
   - UserId
   - FullName
   - Email
   - PhoneNumber
   - DateOfBirth
   - GenderCode
   - JoinDate
   - EmploymentTypeCode
   - EmployeeStatusCode
   - CurrentDepartmentId
   - CurrentPositionId
   - CurrentGradeCode
   - DirectManagerEmployeeId
   - CreatedAt
   - UpdatedAt
4. Department fields:
   - Id
   - Code
   - Name
   - ParentId
   - ManagerEmployeeId
   - IsActive
   - CreatedAt
   - UpdatedAt
5. Position fields:
   - Id
   - Code
   - Name
   - GradeCode
   - IsManagerial
   - IsActive
6. EmployeeDepartmentHistory fields:
   - Id
   - EmployeeId
   - DepartmentId
   - PositionId
   - GradeCode
   - StartDate
   - EndDate
   - IsPrimary
   - ReasonCode
   - CreatedAt

Validation notes:
- EmployeeCode unique.
- Department Code unique.
- Position Code unique.
- Department cannot be parent of itself.
- DirectManagerEmployeeId cannot be the same as EmployeeId.

Permissions:
- hr.employee.view
- hr.employee.create
- hr.employee.update
- hr.employee.delete
- hr.department.view
- hr.department.manage
- hr.position.view
- hr.position.manage

Sensitive permissions:
- hr.employee.personal_info.view
- hr.employee.personal_info.update

Hãy chỉ làm Step 1: Domain & Data trước.
Sau khi hoàn tất, dừng lại và liệt kê file đã tạo/sửa để tôi review.
```

---

## Phase 3 - Backend: Permission DataScope & Sensitive Permission

```text
Hãy triển khai Phase 3 cho HR Permission Extension trong backend Anemoi_Open.

Mục tiêu:
- Không tạo hệ permission HR riêng.
- Kế thừa hệ role/permission hiện tại.
- Mở rộng thêm DataScope, SensitivePermission, RiskLevel và audit khi cấp quyền nhạy cảm.

Scope:
1. Bổ sung metadata cho permission:
   - IsSensitive
   - RiskLevel
   - ModuleCode
2. DataScope:
   - Own
   - Team
   - Department
   - SubDepartments
   - Company
   - SpecificDepartment
   - SpecificEmployee
3. RolePermission hoặc bảng mở rộng cần hỗ trợ ScopeType.
4. Sensitive permission assignment flow:
   - Low: cấp bình thường
   - Medium: confirm + reason
   - High: confirm + reason + re-auth
   - Critical: confirm + reason + re-auth + second approval
5. Audit log cho:
   - assign sensitive permission
   - revoke sensitive permission
   - update role
   - update permission scope

Sensitive HR permissions ban đầu:
- hr.salary.view_all
- hr.salary.update
- hr.salary.adjust
- hr.salary.history.view
- hr.salary.change.approve
- hr.contract.view_all
- hr.contract.create
- hr.contract.update
- hr.contract.terminate
- hr.contract.approve
- hr.employee.personal_info.view
- hr.employee.personal_info.update
- hr.employee.bank_info.view
- hr.employee.bank_info.update
- hr.employee.identity_document.view
- hr.leave.balance.adjust
- hr.leave.request.force_approve
- hr.leave.request.force_cancel
- hr.department.transfer
- hr.position.change
- hr.grade.change
- hr.promotion.approve
- hr.performance.review.view
- hr.performance.review.update
- hr.permission.assign_sensitive

Yêu cầu:
- Không hard-code business role.
- Permission codes phải nằm trong central permission catalog.
- Update seed idempotent.
- Không phá vỡ JWT/permission hiện tại.
- Nếu cần migration, tạo migration rõ ràng.

Hãy phân tích cấu trúc permission hiện tại trước, đề xuất thay đổi tối thiểu, rồi chỉ triển khai Step 1: Domain & Data.
Dừng lại để tôi review.
```

---

## Phase 4 - Backend: Approval Workflow

```text
Hãy triển khai Phase 4 cho Approval Workflow dùng chung trong HR.

Mục tiêu:
- Tạo workflow duyệt reusable.
- Không hard-code trưởng bộ phận.
- Dùng lại cho nghỉ phép, tăng lương, chuyển bộ phận, thăng cấp, hợp đồng, cấp quyền nhạy cảm.

Entities:
1. ApprovalWorkflow
2. ApprovalStep
3. ApprovalRequest
4. ApprovalAction

Fields:
ApprovalWorkflow:
- Id
- Code
- Name
- TargetType
- IsActive

ApprovalStep:
- Id
- WorkflowId
- StepNo
- ApproverType
- ApproverRoleCode
- SpecificEmployeeId
- IsRequired

ApprovalRequest:
- Id
- WorkflowId
- TargetType
- TargetId
- RequesterEmployeeId
- CurrentStepNo
- StatusCode
- CreatedAt
- UpdatedAt

ApprovalAction:
- Id
- ApprovalRequestId
- StepNo
- ApproverEmployeeId
- ActionCode
- Comment
- ActedAt

ApproverType:
- DirectManager
- DepartmentManager
- HRManager
- SpecificEmployee
- Role

Actions:
- Submit
- Approve
- Reject
- RequestChange
- Cancel

Permissions:
- hr.approval.workflow.view
- hr.approval.workflow.manage
- hr.approval.request.view
- hr.approval.request.act

Yêu cầu:
- CQRS đầy đủ.
- Mapperly.
- FluentValidation.
- Stable error codes.
- Localization.
- Publish events khi approval request approved/rejected nếu target module cần.
- Không implement riêng cho nghỉ phép ở phase này, chỉ tạo engine workflow dùng chung.

Hãy làm Step 1: Domain & Data trước, rồi dừng để tôi review.
```

---

## Phase 5 - Backend: Leave Management

```text
Hãy triển khai Phase 5 cho Leave Management trong HR backend.

Mục tiêu:
- Quản lý chính sách ngày phép.
- Quản lý số dư phép bằng transaction ledger.
- Đăng ký nghỉ phép.
- Chọn người duyệt/trưởng bộ phận.
- Kết nối Approval Workflow.
- Chuẩn bị notification/event.

Entities:
1. LeavePolicy
2. EmployeeLeavePolicy
3. LeaveBalance
4. LeaveTransaction
5. LeaveRequest
6. LeaveApproval
7. LeaveAccrualRun

Important:
- Không chỉ update RemainingDays trực tiếp.
- Mọi cộng/trừ/điều chỉnh phép phải ghi LeaveTransaction.
- LeaveAccrualRun phải có unique EmployeeId + YearMonth để chống chạy job trùng.
- MonthlyAccrualDays mặc định 1.25 nhưng phải lấy từ LeavePolicy, không hard-code.

Fields:
LeavePolicy:
- Id
- Code
- Name
- MonthlyAccrualDays
- MaxAnnualDays
- AllowCarryForward
- MaxCarryForwardDays
- IsActive

EmployeeLeavePolicy:
- Id
- EmployeeId
- LeavePolicyId
- StartDate
- EndDate
- IsActive

LeaveBalance:
- Id
- EmployeeId
- Year
- OpeningDays
- AccruedDays
- UsedDays
- PendingDays
- AdjustedDays
- RemainingDays

LeaveTransaction:
- Id
- EmployeeId
- Type
- Days
- SourceType
- SourceId
- Description
- CreatedAt

LeaveRequest:
- Id
- EmployeeId
- LeaveTypeCode
- StartDate
- EndDate
- TotalDays
- Reason
- StatusCode
- CurrentApproverEmployeeId
- CreatedAt
- UpdatedAt

LeaveApproval:
- Id
- LeaveRequestId
- StepNo
- ApproverEmployeeId
- StatusCode
- Comment
- ActedAt

LeaveAccrualRun:
- Id
- EmployeeId
- YearMonth
- Days
- StatusCode
- CreatedAt

Permissions:
- hr.leave.request.create
- hr.leave.request.view_own
- hr.leave.request.view_team
- hr.leave.request.view_department
- hr.leave.request.view_all
- hr.leave.request.approve
- hr.leave.request.reject
- hr.leave.request.cancel
- hr.leave.request.force_approve
- hr.leave.request.force_cancel
- hr.leave.balance.view
- hr.leave.balance.adjust
- hr.leave.policy.manage

Sensitive:
- hr.leave.balance.adjust
- hr.leave.request.force_approve
- hr.leave.request.force_cancel

Events:
- LeaveRequestSubmitted
- LeaveRequestApproved
- LeaveRequestRejected
- LeaveRequestCancelled
- LeaveBalanceChanged

Hãy làm Step 1: Domain & Data trước.
Dừng lại để tôi review.
```

---

## Phase 6 - Backend: Monthly Leave Accrual Worker

```text
Hãy triển khai Phase 6 cho Monthly Leave Accrual Worker.

Mục tiêu:
- Tự động cộng ngày phép cuối mỗi tháng.
- Mặc định 1.25 ngày/tháng, lấy từ LeavePolicy.
- Chống chạy trùng.
- Ghi transaction ledger.
- Publish event.

Yêu cầu nghiệp vụ:
1. Job chạy vào cuối tháng.
2. Lấy danh sách Employee active.
3. Lấy EmployeeLeavePolicy active.
4. Tính số ngày cộng:
   - policy.MonthlyAccrualDays
   - default expected = 1.25
5. Nếu EmployeeId + YearMonth đã tồn tại trong LeaveAccrualRuns thì skip.
6. Nếu chưa tồn tại:
   - tạo LeaveAccrualRun
   - tạo LeaveTransaction Type = Accrual
   - update LeaveBalance
   - publish MonthlyLeaveAccrued
7. Job phải idempotent.
8. Log bằng Serilog.
9. Dùng Polly nếu có external dependency cần retry.
10. Không hard-code localized message.

Hãy triển khai theo đúng architecture:
- WorkerService hoặc hosted service phù hợp với cấu trúc repo.
- Application command cho accrual logic.
- Infrastructure chỉ chứa implementation technical.
- Không đặt business logic trong Program.cs.

Dừng lại sau khi hoàn tất phần implementation plan và Step 1/Domain nếu cần schema bổ sung.
```

---

## Phase 7 - Frontend: HR Foundation & Master Data UI

```text
Hãy triển khai Phase 7 cho frontend HR trong repo cody-web-app.

Tuân thủ frontend-guidelines.md.

Mục tiêu:
- Tạo cấu trúc frontend HR.
- Tạo UI quản lý Master Data / Category.
- Tích hợp permission-based route/button visibility nếu hệ thống hiện tại đã có.

Scope:
1. Tạo routes:
   - /[locale]/hr/master-data
2. Tạo folders:
   - components/features/hr/master-data
   - services/hr
   - hooks/hr
   - types/hr
   - constants/hr
3. Tạo TypeScript interfaces:
   - CategoryGroupDto
   - CategoryDto
   - CreateCategoryGroupRequest
   - UpdateCategoryGroupRequest
   - CreateCategoryRequest
   - UpdateCategoryRequest
4. Tạo service:
   - hr-master-data-service.ts
5. Tạo hooks:
   - useCategoryGroups
   - useCategoriesByGroup
   - useCreateCategoryGroup
   - useUpdateCategoryGroup
   - useCreateCategory
   - useUpdateCategory
6. UI:
   - Category group list
   - Category list by group
   - Tree view theo ParentId
   - Create/update dialog
   - Active/inactive badge
7. i18n:
   - cập nhật messages/vi.json
   - cập nhật messages/en.json
8. Error handling:
   - không duplicate toast lỗi HTTP đã được interceptor xử lý
9. Layout:
   - main wrapper: flex flex-col gap-3 w-full
   - compact cards, tables

Không dùng `any`.
Không hard-code UI text.
Sau khi xong, liệt kê file đã tạo/sửa.
```

---

## Phase 8 - Frontend: Employee & Organization UI

```text
Hãy triển khai Phase 8 cho frontend HR Employee & Organization.

Tuân thủ frontend-guidelines.md.

Routes:
- /[locale]/hr/employees
- /[locale]/hr/employees/[id]
- /[locale]/hr/departments
- /[locale]/hr/positions

Scope:
1. Types:
   - EmployeeDto
   - DepartmentDto
   - PositionDto
   - EmployeeDepartmentHistoryDto
2. Services:
   - hr-employee-service.ts
   - hr-organization-service.ts
3. Hooks:
   - useEmployees
   - useEmployeeDetail
   - useDepartments
   - useDepartmentTree
   - usePositions
4. UI:
   - Employee table
   - Employee detail page
   - Department tree
   - Position list
   - Status badges
   - Permission-based buttons
5. Forms:
   - Create/update employee
   - Create/update department
   - Create/update position
   - React Hook Form + Zod
6. i18n:
   - vi/en đầy đủ
7. Không hiển thị sensitive personal data nếu user không có permission tương ứng.

Không dùng `any`.
Không hard-code UI text.
Không duplicate API error toast.
Sau khi xong, liệt kê file đã tạo/sửa.
```

---

## Phase 9 - Frontend: Leave Request & Approval UI

```text
Hãy triển khai Phase 9 cho frontend Leave Management.

Tuân thủ frontend-guidelines.md.

Routes:
- /[locale]/hr/leave/my-requests
- /[locale]/hr/leave/requests
- /[locale]/hr/leave/approvals
- /[locale]/hr/leave/balances

Scope:
1. Types:
   - LeavePolicyDto
   - LeaveBalanceDto
   - LeaveTransactionDto
   - LeaveRequestDto
   - LeaveApprovalDto
   - CreateLeaveRequestRequest
   - ApproveLeaveRequestRequest
   - RejectLeaveRequestRequest
2. Services:
   - hr-leave-service.ts
3. Hooks:
   - useMyLeaveRequests
   - useLeaveRequests
   - useLeaveApprovals
   - useLeaveBalance
   - useCreateLeaveRequest
   - useApproveLeaveRequest
   - useRejectLeaveRequest
   - useCancelLeaveRequest
4. UI:
   - My leave requests table
   - Create leave request form
   - Select leave type
   - Select date range
   - Show calculated total days
   - Select approver/trưởng bộ phận
   - Approval inbox
   - Approve/reject dialog with comment
   - Leave balance card
   - Leave transaction history
5. Permission:
   - employee chỉ xem own request
   - manager xem team/dept tùy DataScope
   - HR xem theo permission
6. i18n:
   - messages/vi.json
   - messages/en.json
7. Error handling:
   - không duplicate toast từ interceptor
8. UX:
   - skeleton loader
   - disabled state khi submitting
   - empty state
   - compact table/card layout

Không dùng `any`.
Không hard-code UI text.
Sau khi xong, liệt kê file đã tạo/sửa.
```

---

## Phase 10 - Frontend: Sensitive Permission Management UI

```text
Hãy triển khai Phase 10 cho frontend Sensitive Permission Management.

Tuân thủ frontend-guidelines.md.

Route:
- /[locale]/hr/permissions/sensitive

Mục tiêu:
- Tránh cấp nhầm quyền nhạy cảm.
- Hiển thị cảnh báo rõ ràng.
- Bắt nhập lý do.
- Với High/Critical risk, yêu cầu re-auth hoặc tạo approval request tùy backend API.

UI flow:
1. Admin/HR chọn user hoặc role group.
2. Chọn permission.
3. Nếu permission IsSensitive:
   - Hiển thị warning panel
   - Hiển thị RiskLevel
   - Bắt nhập reason
   - Bắt confirm checkbox:
     "Tôi hiểu đây là quyền nhạy cảm và có thể ảnh hưởng đến dữ liệu nhân sự."
   - Nếu High/Critical:
     - gọi API re-auth hoặc submit approval request
4. Show audit trail nếu có permission.

Scope:
1. Types:
   - PermissionDto
   - SensitivePermissionAssignmentRequest
   - SensitivePermissionAuditDto
2. Service:
   - hr-sensitive-permission-service.ts
3. Hooks:
   - useSensitivePermissions
   - useAssignSensitivePermission
   - useRevokeSensitivePermission
   - useSensitivePermissionAudits
4. UI:
   - sensitive permission list
   - risk badge
   - assignment dialog
   - reason input
   - confirmation section
   - audit table

Không dùng `any`.
Không hard-code UI text.
Không duplicate API error toast.
Sau khi xong, liệt kê file đã tạo/sửa.
```

---

# 12. Prompt kiểm tra chất lượng sau mỗi phase

Dùng prompt này sau khi Codex tạo code.

```text
Hãy review lại toàn bộ thay đổi của phase vừa rồi.

Checklist backend:
- Có đúng Clean Architecture không?
- Controller có query DB trực tiếp không?
- Có đúng CQRS Command/Query/Handler không?
- Có dùng Mapperly không?
- Có FluentValidation không?
- Có OneOf return pattern không?
- Có strongly-typed IDs không?
- Có hard-code user-facing message không?
- Có stable error code không?
- Có permission bảo vệ endpoint không?
- Permission có nằm trong central catalog không?
- Có update seed permission không?
- Có migration phù hợp không?
- Có phá vỡ contract/JWT/permission hiện tại không?

Checklist frontend:
- Có dùng TypeScript strict không?
- Có `any` không?
- UI text có nằm trong messages/vi.json và messages/en.json không?
- API call có nằm trong services không?
- Business query/mutation có nằm trong hooks không?
- Component có duplicate toast lỗi từ interceptor không?
- Có React Hook Form + Zod cho form không?
- Có skeleton/disabled/empty state không?
- Layout có đúng compact guideline không?

Hãy liệt kê:
1. Điểm đúng
2. Điểm chưa đúng
3. File cần sửa
4. Đề xuất patch tiếp theo
Không tự sửa nếu tôi chưa yêu cầu.
```

---

# 13. Prompt mở rộng sau này

## Payroll

```text
Thiết kế Payroll module cho HR Platform.

Yêu cầu:
- Salary profile
- Salary history
- Allowance
- Deduction
- Gross/net salary
- Salary change request
- Approval workflow
- Sensitive permissions
- Audit log
- Không expose salary data nếu không có permission và DataScope phù hợp

Tuân thủ ArchitectureGuide.md và các pattern HR đã có.
Hãy bắt đầu bằng implementation plan, chưa code.
```

## Contract

```text
Thiết kế Contract module cho HR Platform.

Yêu cầu:
- Employee contracts
- Contract type/status từ MasterData
- Contract document file
- Contract expiry reminder
- Contract renewal
- Contract termination
- Approval workflow
- Sensitive permissions
- Audit log

Tuân thủ ArchitectureGuide.md và các pattern HR đã có.
Hãy bắt đầu bằng implementation plan, chưa code.
```

## Skill & Career Path

```text
Thiết kế Skill, Grade, Career Path và Promotion module cho HR Platform.

Yêu cầu:
- Skill category
- Skill level
- Employee skill matrix
- Grade/level
- Career path
- Promotion request
- Performance review
- Approval workflow
- Audit log

Tuân thủ ArchitectureGuide.md và các pattern HR đã có.
Hãy bắt đầu bằng implementation plan, chưa code.
```

---

# 14. Nguyên tắc cuối cùng

1. HR không được tách permission system riêng.
2. Kế thừa permission hiện tại, mở rộng DataScope và SensitivePermission.
3. Quyền nhạy cảm phải có RiskLevel, reason, re-auth/approval nếu cần.
4. LeaveBalance phải quản lý bằng transaction ledger.
5. Cộng phép cuối tháng phải idempotent.
6. Không hard-code trưởng bộ phận trong nghỉ phép.
7. Approval Workflow phải dùng lại được cho nhiều nghiệp vụ.
8. Category/MasterData phải có GroupCode, ParentId, MetadataJson.
9. Backend phải đi đúng Clean Architecture + CQRS.
10. Frontend phải đi đúng Next.js + i18n + service/hooks separation.
11. Không làm một lần quá lớn. Chia phase nhỏ, review từng bước.

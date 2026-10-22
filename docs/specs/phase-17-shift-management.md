# Phase 17 - Shift Management (Quản lý Ca làm)

> **Mục tiêu:** Xây dựng module quản lý ca làm (Shift Management) cho ANEMOI HR — nền tảng cho việc sắp xếp lịch làm việc, chấm công, tích hợp lịch nghỉ lễ trong tương lai, và tính toán overtime.

---

## 1. Domain Layer

### Strongly Typed IDs (`Anemoi.Hr.ModelIds/ModelIds/`)

| File | Kiểu |
|------|------|
| `ShiftTemplateId.cs` | `sealed record ShiftTemplateId(Guid Value) : StronglyTypedId<Guid>` |
| `EmployeeShiftAssignmentId.cs` | `sealed record EmployeeShiftAssignmentId(Guid Value) : StronglyTypedId<Guid>` |

### Entities (`Anemoi.Hr.Domain/ShiftManagement/`)

#### `ShiftTemplate.cs`
- Aggregate root cho "mẫu ca" (shift template)
- Factory method `Create()` với validation: Code/Name không rỗng, `startTime < endTime`, `breakMinutes >= 0`
- `Update()` — cập nhật thông tin mẫu ca
- `Activate()` / `Deactivate()` — bật/tắt mẫu ca
- `CalculateExpectedWorkingHours()` — tính giờ làm việc kỳ vọng = (endTime - startTime - breakMinutes) / 60, làm tròn 2 chữ số
- `IsActive` mặc định `true` khi tạo
- Không cho phép hard-delete (chỉ deactivate)

#### `EmployeeShiftAssignment.cs`
- Aggregate root cho "phân ca" (shift assignment)
- Snapshot pattern: lưu giá trị của ShiftTemplate tại thời điểm tạo (ShiftNameSnapshot, StartTimeSnapshot, EndTimeSnapshot, BreakMinutesSnapshot, ExpectedWorkingHoursSnapshot)
- Factory method `Create()` với validation: employeeId không null, shiftTemplate không null + phải active, assignedBy không rỗng
- `Cancel()` — chuyển trạng thái từ `Assigned` → `Cancelled`, ghi nhận cancelledBy/cancelledAt/cancellationReason
- State machine: chỉ cho phép Cancel khi đang ở trạng thái `Assigned`

#### `EmployeeShiftAssignmentStatus.cs`
- Constants: `Assigned = "Assigned"`, `Cancelled = "Cancelled"`

---

## 2. Application Layer

### Mapper (`Anemoi.Hr.Application/Mappings/ShiftManagementMapper.cs`)
- `ToShiftTemplateResponse()` — map entity → response (manual mapping, không dùng Mapperly auto)
- `ToShiftTemplateIdResponse()` — response chứa ID + CreatedAt
- `ToEmployeeShiftAssignmentResponse()` — map entity → response (bao gồm EmployeeCode, EmployeeName, ShiftTemplateCode từ navigation properties)

### Responses (`Anemoi.Hr.Application/Responses/`)

| File | Mục đích |
|------|----------|
| `ShiftTemplateResponse.cs` | Response chi tiết mẫu ca (Id là Guid, thời gian là TimeOnly, ...) |
| `ShiftTemplateIdResponse.cs` | Response tạo mẫu ca (Id + CreatedAt) |
| `EmployeeShiftAssignmentResponse.cs` | Response phân ca (kèm EmployeeCode, EmployeeName, các snapshot giá trị) |
| `EmployeeScheduleCalendarResponse.cs` | Response lịch làm việc (group theo employee, danh sách CalendarDayResponse) |

### Commands & Handlers (7 commands)

| Command | Handler | Validator | Mô tả |
|---------|---------|-----------|-------|
| `CreateShiftTemplate` | Kiểm tra code trùng → `ShiftTemplate.Create()` → lưu → trả về response | Code not empty+max50, Name not empty+max200, StartTime < EndTime, BreakMinutes >= 0 | Tạo mẫu ca mới |
| `UpdateShiftTemplate` | Tìm template → kiểm tra code trùng (trừ chính nó) → `template.Update()` | Giống CreateValidator | Cập nhật mẫu ca |
| `ActivateShiftTemplate` | Tìm template → `template.Activate()` | Không có | Kích hoạt mẫu ca |
| `DeactivateShiftTemplate` | Tìm template → `template.Deactivate()` | Không có | Vô hiệu hóa mẫu ca |
| `AssignShiftToEmployee` | Validate employee tồn tại → validate template active → kiểm tra trùng → `EmployeeShiftAssignment.Create()` | ShiftTemplateId not null, AssignedBy not empty | Phân ca cho 1 nhân viên |
| `BulkAssignShift` | Validate template → lọc employeeIds hợp lệ + dedup → kiểm tra trùng → tạo hàng loạt | EmployeeIds not empty, ShiftTemplateId not null | Phân ca hàng loạt |
| `CancelEmployeeShiftAssignment` | Tìm assignment → `assignment.Cancel()` | CancelledBy not empty, CancellationReason not empty+max500 | Hủy phân ca |

### Queries & Handlers (5 queries)

| Query | Handler | Mô tả |
|-------|---------|-------|
| `GetShiftTemplates` | Phân trang, filter theo IsActive + Search (Code/Name), sort | Danh sách mẫu ca |
| `GetShiftTemplateById` | Tìm theo ID | Chi tiết mẫu ca |
| `GetEmployeeShiftAssignments` | Phân trang, filter theo EmployeeId/ShiftTemplateId/WorkDate/Status, sort | Danh sách phân ca (Include Employee + ShiftTemplate) |
| `GetEmployeeShiftAssignmentById` | Tìm theo ID | Chi tiết phân ca |
| `GetEmployeeScheduleCalendar` | Filter theo EmployeeId/FromDate/ToDate, trả về grouped theo employee | Lịch làm việc dạng tháng |

### Abstraction
- `IShiftScheduleSnapshotProvider` — interface dự phòng cho tích hợp Attendance/Payroll trong tương lai

---

## 3. API Layer

### Controller (`Anemoi.Hr.Api/Controllers/ShiftManagement/ShiftManagementController.cs`)

Route: `api/hr/shift-management/[action]`

| Method | Endpoint | Permission | Mô tả |
|--------|----------|------------|-------|
| GET | `GetShiftTemplates` | `ShiftView` | Danh sách template (phân trang) |
| GET | `GetShiftTemplateById/{id}` | `ShiftView` | Chi tiết template |
| POST | `CreateShiftTemplate` | `ShiftManage` | Tạo template |
| PUT | `UpdateShiftTemplate/{id}` | `ShiftManage` | Cập nhật template |
| POST | `ActivateShiftTemplate/{id}` | `ShiftManage` | Kích hoạt template |
| POST | `DeactivateShiftTemplate/{id}` | `ShiftManage` | Vô hiệu hóa template |
| GET | `GetEmployeeShiftAssignments` | `ShiftView` | Danh sách phân ca (phân trang) |
| GET | `GetEmployeeShiftAssignmentById/{id}` | `ShiftView` | Chi tiết phân ca |
| POST | `AssignShiftToEmployee` | `ShiftAssign` | Phân ca 1 nhân viên |
| POST | `BulkAssignShift` | `ShiftAssign` | Phân ca hàng loạt |
| POST | `CancelEmployeeShiftAssignment/{id}` | `ShiftCancel` | Hủy phân ca |
| GET | `GetEmployeeScheduleCalendar` | `ShiftView` | Lịch làm việc tháng |

### Error Codes (`Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs` — 13 codes mới)

| Code | Ý nghĩa |
|------|---------|
| `HR_SHIFT_TEMPLATE_NOT_FOUND` | Không tìm thấy mẫu ca |
| `HR_SHIFT_TEMPLATE_CODE_ALREADY_EXISTS` | Mã mẫu ca đã tồn tại |
| `HR_SHIFT_TEMPLATE_INVALID_TIME_RANGE` | Khung giờ không hợp lệ |
| `HR_SHIFT_TEMPLATE_INVALID_BREAK_MINUTES` | Số phút nghỉ không hợp lệ |
| `HR_SHIFT_TEMPLATE_INACTIVE` | Mẫu ca đang bị vô hiệu hóa |
| `HR_SHIFT_ASSIGNMENT_NOT_FOUND` | Không tìm thấy phân ca |
| `HR_SHIFT_ASSIGNMENT_DUPLICATE` | Nhân viên đã có ca trong ngày |
| `HR_SHIFT_ASSIGNMENT_OVERLAP` | Trùng lịch ca |
| `HR_SHIFT_ASSIGNMENT_INVALID_STATUS` | Trạng thái phân ca không hợp lệ |
| `HR_SHIFT_TEMPLATE_CODE_REQUIRED` | Mã mẫu ca bắt buộc |
| `HR_SHIFT_TEMPLATE_NAME_REQUIRED` | Tên mẫu ca bắt buộc |
| `HR_SHIFT_ASSIGNMENT_CONCURRENCY_CONFLICT` | Xung đột đồng thời phân ca |
| `HR_SHIFT_TEMPLATE_CONCURRENCY_CONFLICT` | Xung đột đồng thời mẫu ca |

### Permissions (`Anemoi.Hr.Application/Configurations/HrPermissions.cs`)

| Constant | Permission String | Group (Resource) |
|----------|------------------|------------------|
| `ShiftView` → `hr.shift.view` | `Xem` | `PermissionGroupHrShift` |
| `ShiftManage` → `hr.shift.manage` | `Quản lý` | `PermissionGroupHrShift` |
| `ShiftAssign` → `hr.shift.assign` | `Phân ca` | `PermissionGroupHrShift` |
| `ShiftCancel` → `hr.shift.cancel` | `Hủy ca` | `PermissionGroupHrShift` |

Được định nghĩa ở `Anemoi.BuildingBlock.Application/Authorization/Permissions.cs` với metadata (group + description resource keys).

---

## 4. Infrastructure Layer

### EF Core Config (`Anemoi.Hr.Infrastructure/Configurations/ShiftManagementModelMapping.cs`)

**Table:** `ShiftTemplates`
- PK: `Id` (uuid)
- Columns: Code (varchar 50, unique), Name (varchar 200), StartTime (time), EndTime (time), BreakMinutes (int), ExpectedWorkingHours (decimal 9,2), IsActive (bool), CreatedAt/UpdatedAt (timestamptz)
- Indexes: `IX_ShiftTemplates_Code` (unique), `IX_ShiftTemplates_IsActive`
- Concurrency: xmin (row version)

**Table:** `EmployeeShiftAssignments`
- PK: `Id` (uuid)
- Columns: EmployeeId (FK → Employees), ShiftTemplateId (FK → ShiftTemplates), WorkDate (date), ShiftNameSnapshot, StartTimeSnapshot, EndTimeSnapshot, BreakMinutesSnapshot, ExpectedWorkingHoursSnapshot, Status (varchar 20), AssignedBy (varchar 128), AssignedAt (timestamptz), CancelledBy (varchar 128, nullable), CancelledAt (timestamptz, nullable), CancellationReason (varchar 500, nullable)
- FK: `EmployeeId` → `Employees` (Restrict), `ShiftTemplateId` → `ShiftTemplates` (Restrict)
- Indexes: `IX_EmployeeShiftAssignments_EmployeeId_WorkDate_Status`, `IX_EmployeeShiftAssignments_ShiftTemplateId`, `IX_EmployeeShiftAssignments_WorkDate`, `IX_EmployeeShiftAssignments_Status`, `IX_EmployeeShiftAssignments_CreatedAt`
- Concurrency: xmin (row version)

### Migration
- File: `Anemoi.Hr.Infrastructure/Migrations/20250612000001_AddShiftManagement.cs`
- Tạo 2 bảng + indexes + FK constraints

### DbContext Registration
- `DbSet<ShiftTemplate> ShiftTemplates`
- `DbSet<EmployeeShiftAssignment> EmployeeShiftAssignments`

### DI Registration
- File: `Anemoi.Hr.Infrastructure/Installers/ShiftManagementServiceInstaller.cs`
- Đăng ký repositories và các dịch vụ liên quan

---

## 5. Frontend (cody-web-app)

### Types (`src/types/hr/shiftManagement.ts`)
- `ShiftTemplate`, `CreateShiftTemplateRequest`, `UpdateShiftTemplateRequest`
- `EmployeeShiftAssignment`, `ShiftAssignmentStatus` ('Assigned' | 'Cancelled')
- `AssignShiftRequest`, `BulkAssignShiftRequest`, `CancelShiftAssignmentRequest`
- `EmployeeScheduleCalendarResponse`, `CalendarDay`
- `PaginationResponse<T>`

### Service (`src/services/hr/shiftManagementService.ts`)
- 11 API methods mapping trực tiếp đến controller endpoints
- Dùng axios với URL pattern `/api/hr/shift-management/{ActionName}`

### React Query Hooks (`src/hooks/hr/useShiftManagement.ts`)
- 5 query hooks: `useShiftTemplates`, `useShiftTemplate`, `useEmployeeShiftAssignments`, `useEmployeeShiftAssignment`, `useEmployeeScheduleCalendar`
- 5 mutation hooks: `useCreateShiftTemplate`, `useUpdateShiftTemplate`, `useActivateShiftTemplate`, `useDeactivateShiftTemplate`, `useAssignShiftToEmployee`, `useBulkAssignShift`, `useCancelEmployeeShiftAssignment`
- Query key pattern: `['shiftTemplates', params]`, `['shiftAssignments', params]`, `['shiftCalendar', params]`
- Invalidates queries on mutations (assignments + calendar khi assign/bulk/cancel)

### Page (`src/app/[locale]/(dashboard)/hr/shifts/page.tsx`)

3 tabs trong layout dashboard HR:

1. **Shift Templates Tab**
   - Bảng danh sách mẫu ca với search + refresh
   - Dialog create/edit form (code, name, start/end time, break minutes)
   - Nút activate/deactivate toggle với icon
   - Phân quyền: chỉ hiện nút quản lý nếu có `ShiftManage`

2. **Assignments Tab**
   - Bảng danh sách phân ca với filter theo status (Assigned/Cancelled)
   - Dialog assign 1 employee (chọn template từ dropdown, nhập employeeId, chọn ngày)
   - Dialog bulk assign (nhập nhiều employeeIds dạng CSV)
   - Nút cancel assignment (chỉ khi status là Assigned + có `ShiftCancel`)
   - Phân quyền: ẩn/hiện nút assign/bulk/cancel

3. **Schedule Calendar Tab**
   - Grid lịch tháng: hang = employee, cột = ngày trong tháng
   - Navigation previous/next month
   - Highlight ngày hiện tại
   - Hiển thị tên ca trong ô, gạch ngang nếu đã hủy
   - Tooltip hiển thị giờ làm

### Permissions Constants (`src/constants/permissions.ts`)
- `HR_SHIFT_VIEW = "hr.shift.view"`
- `HR_SHIFT_MANAGE = "hr.shift.manage"`
- `HR_SHIFT_ASSIGN = "hr.shift.assign"`
- `HR_SHIFT_CANCEL = "hr.shift.cancel"`
- Route guard: `/hr/shifts` → `[HR_SHIFT_VIEW]`

### Sidebar (`src/components/shared/Sidebar.tsx`)
- Entry `hrShifts` với icon `CalendarDays` trong nhóm HR

---

## 6. Tests

**28 tests mới** trong `Anemoi.BuildingBlock.Test/HrShiftManagementTests.cs`

| Test | Mô tả |
|------|-------|
| `CreateValidTemplate_SetsPropertiesCorrectly` | Tạo template hợp lệ |
| `CreateTemplate_WithoutCode_ThrowsException` | Code rỗng → exception |
| `CreateTemplate_WithoutName_ThrowsException` | Name rỗng → exception |
| `CreateTemplate_WithInvalidTimeRange_ThrowsException` | Start >= End → exception |
| `CreateTemplate_WithNegativeBreakMinutes_ThrowsException` | BreakMinutes âm → exception |
| `CalculateExpectedWorkingHours_CorrectCalculation` | Tính giờ kỳ vọng đúng |
| `UpdateTemplate_UpdatesProperties` | Update dữ liệu mới |
| `UpdateTemplate_WithInvalidTimeRange_ThrowsException` | Update với time không hợp lệ |
| `ActivateTemplate_SetsActive` | Activate thành công |
| `DeactivateTemplate_SetsInactive` | Deactivate thành công |
| `Deactivate_AlreadyInactive_RemainsInactive` | Deactivate khi đã inactive |
| `ActiveTemplate_ByDefault_IsActive` | Mặc định active |
| `CreateAssignment_WithValidData_SetsCorrectProperties` | Tạo phân ca hợp lệ |
| `CreateAssignment_StoresSnapshotValues` | Snapshot lưu đúng giá trị |
| `CreateAssignment_WithNullEmployeeId_ThrowsException` | EmployeeId null → exception |
| `CreateAssignment_WithNullShiftTemplate_ThrowsException` | ShiftTemplate null → exception |
| `CreateAssignment_WithInactiveTemplate_ThrowsException` | Template inactive → exception |
| `CreateAssignment_WithoutAssignedBy_ThrowsException` | AssignedBy rỗng → exception |
| `CancelAssignment_ChangesStatusAndSetsCancelledBy` | Hủy phân ca thành công |
| `Cancel_AlreadyCancelled_ThrowsException` | Hủy ca đã hủy → exception |
| `CancelAssignment_SetsCancelledAtUtc` | Kiểm tra thời gian hủy |
| `EditingTemplate_DoesNotMutateExistingAssignmentSnapshots` | Edit template không ảnh hưởng snapshot cũ |
| `MultipleAssignments_ToDifferentEmployees_AreIndependent` | Nhiều phân ca độc lập |
| `MultipleAssignments_ToSameEmployeeDifferentDates_AreIndependent` | Cùng employee khác ngày |

---

## 7. Localization

### Backend Resources

**`SharedResource.en.resx`** — 4 permission descriptions bằng tiếng Anh:
```
PermissionGroupHrShift = "HR Shift Management"
PermissionDescriptionHrShiftView = "View shift templates, assignments and schedule calendar."
PermissionDescriptionHrShiftManage = "Create, update, activate and deactivate shift templates."
PermissionDescriptionHrShiftAssign = "Assign and bulk assign shifts to employees."
PermissionDescriptionHrShiftCancel = "Cancel employee shift assignments."
```

**`SharedResource.vi.resx`** — 5 entries bằng tiếng Việt:
```
PermissionGroupHrShift = "Quản lý Ca làm"
PermissionDescriptionHrShiftView = "Xem mẫu ca, phân ca và lịch làm việc."
PermissionDescriptionHrShiftManage = "Tạo, cập nhật, kích hoạt và vô hiệu hóa mẫu ca."
PermissionDescriptionHrShiftAssign = "Phân ca cho nhân viên."
PermissionDescriptionHrShiftCancel = "Hủy phân ca cho nhân viên."
```

---

## 8. Architectural Decisions

1. **Snapshot pattern** — `EmployeeShiftAssignment` lưu giá trị của ShiftTemplate tại thời điểm tạo; thay đổi template sau đó không ảnh hưởng assignment cũ
2. **No hard-delete** — ShiftTemplate chỉ deactivate (IsActive = false), Assignment chỉ cancel (chuyển status)
3. **Same-day shifts** — MVP chỉ hỗ trợ ca trong ngày (không cross-midnight)
4. **PostgreSQL xmin** — Sử dụng xmin cho concurrency thay vì RowVersion, đồng bộ với các module khác
5. **No overtime/payroll coupling** — Shift Management là module độc lập; `IShiftScheduleSnapshotProvider` interface dự phòng cho tích hợp tương lai
6. **Table naming** — Sử dụng `ShiftTemplates` và `EmployeeShiftAssignments` (PascalCase), đồng bộ với convention chung của HR module

---

## 9. Code Review Fixes

1. **SharedResource.vi.resx** — Khôi phục dấu tiếng Việt cho 5 shift permission labels
2. **CancelEmployeeShiftAssignmentValidator** — Thêm validation `CancellationReason` (required + max 500)
3. **HrShiftManagementTests.cs** — Fix xUnit2002 warning (`Assert.NotNull` trên value type `DateTime`)
4. **BulkAssignShiftHandler** — Cross-check employee tồn tại qua DB, deduplicate input list
5. **AssignShiftToEmployeeHandler** — Loại bỏ dead null check, thêm assignedBy validation

---

## 10. File Inventory

### New Files (52 files)

**Domain (5):**
- `Anemoi.Hr.Domain/ShiftManagement/ShiftTemplate.cs`
- `Anemoi.Hr.Domain/ShiftManagement/EmployeeShiftAssignment.cs`
- `Anemoi.Hr.Domain/ShiftManagement/EmployeeShiftAssignmentStatus.cs`

**ModelIds (2):**
- `Anemoi.Hr.ModelIds/ModelIds/ShiftTemplateId.cs`
- `Anemoi.Hr.ModelIds/ModelIds/EmployeeShiftAssignmentId.cs`

**Application - CQRS Commands (21 files — 7 commands × 3 files):**
- `CreateShiftTemplate/` (Command, Handler, Validator)
- `UpdateShiftTemplate/` (Command, Handler, Validator)
- `ActivateShiftTemplate/` (Command, Handler)
- `DeactivateShiftTemplate/` (Command, Handler)
- `AssignShiftToEmployee/` (Command, Handler, Validator)
- `BulkAssignShift/` (Command, Handler, Validator)
- `CancelEmployeeShiftAssignment/` (Command, Handler, Validator)

**Application - CQRS Queries (10 files — 5 queries × 2 files):**
- `GetShiftTemplates/` (Query, Handler)
- `GetShiftTemplateById/` (Query, Handler)
- `GetEmployeeShiftAssignments/` (Query, Handler)
- `GetEmployeeShiftAssignmentById/` (Query, Handler)
- `GetEmployeeScheduleCalendar/` (Query, Handler)

**Application - Other (8 files):**
- `Mappings/ShiftManagementMapper.cs`
- `Responses/ShiftTemplateResponse.cs`
- `Responses/ShiftTemplateIdResponse.cs`
- `Responses/EmployeeShiftAssignmentResponse.cs`
- `Responses/EmployeeScheduleCalendarResponse.cs`
- `Abstractions/IShiftScheduleSnapshotProvider.cs`
- `Configurations/HrSettings.cs`

**API (1 file):**
- `Controllers/ShiftManagement/ShiftManagementController.cs`

**Infrastructure (4 files):**
- `Configurations/ShiftManagementModelMapping.cs`
- `Installers/ShiftManagementServiceInstaller.cs`
- `Migrations/20250612000001_AddShiftManagement.cs`

**Tests (1 file):**
- `Anemoi.BuildingBlock.Test/HrShiftManagementTests.cs` (28 tests)

**Frontend (4 files):**
- `cody-web-app/src/types/hr/shiftManagement.ts`
- `cody-web-app/src/services/hr/shiftManagementService.ts`
- `cody-web-app/src/hooks/hr/useShiftManagement.ts`
- `cody-web-app/src/app/[locale]/(dashboard)/hr/shifts/page.tsx`

### Modified Files (6 files)

- `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Authorization/Permissions.cs` — thêm 4 shift permissions + metadata
- `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Resources/SharedResource.en.resx` — thêm 5 entries
- `Anemoi.BuildingBlocks/Anemoi.BuildingBlock.Application/Resources/SharedResource.vi.resx` — thêm 5 entries
- `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrBusinessErrorCodes.cs` — thêm 13 codes
- `Anemoi.Hr/Anemoi.Hr.Application/Configurations/HrPermissions.cs` — thêm 4 constants + All list
- `Anemoi.Hr/Anemoi.Hr.Infrastructure/Persistence/HrDbContext.cs` — thêm 2 DbSets
- `cody-web-app/src/components/shared/Sidebar.tsx` — thêm nav item
- `cody-web-app/src/constants/permissions.ts` — thêm 4 permission constants + route guard

---

## 11. Verification

- `dotnet build Anemoi.sln` → **0 errors, 0 warnings**
- `dotnet test Anemoi.BuildingBlock.Test` → **134/134 passed** (106 cũ + 28 mới)
- `npm run lint` (cody-web-app) → **0 errors** (6 warnings pre-existing)
- `npm run build` (cody-web-app) → **PASS**, route `/hr/shifts` registered

---

## 12. Known Limitations (MVP)

1. Cross-midnight shifts chưa được hỗ trợ
2. Chưa có recurring patterns (lặp lịch hàng tuần/tháng)
3. Frontend i18n messages (`HRShiftManagement` keys, `hrShifts` sidebar label) chưa được thêm vào `messages/vi.json` và `messages/en.json`
4. `IShiftScheduleSnapshotProvider` interface được định nghĩa nhưng chưa implement (dành cho Phase tích hợp Attendance/Payroll sau)

# Localization Rules

## Backend

Supported cultures:

```text
vi-VN
en-US
```

Default culture:

```text
vi-VN
```

APIs must read language from:

```text
Accept-Language
```

User-facing messages must be localized.

Do not hard-code Vietnamese or English messages in:

```text
Controllers
Handlers
Validators
Middleware
Filters
```

Business errors must use stable error codes.

Example:

```text
HR_LEAVE_BALANCE_NOT_ENOUGH
HR_EMPLOYEE_NOT_FOUND
HR_LEAVE_REQUEST_ALREADY_APPROVED
HR_PERMISSION_SENSITIVE_CONFIRMATION_REQUIRED
```

## Frontend

Supported locales:

```text
vi
en
```

Mapping:

```text
vi -> vi-VN
en -> en-US
```

All UI strings must be defined in:

```text
messages/vi.json
messages/en.json
```

Do not translate machine-readable backend codes directly. Map them to localized labels in the UI.

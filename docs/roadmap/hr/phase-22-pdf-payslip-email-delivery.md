# Phase 22 - PDF Payslip & Email Delivery

Status: Implemented

---

## Objective

Generate PDF payslips and optionally deliver them by email. This phase extends Phase 14 Payslip MVP.

---

## Business Requirements

- Generate PDF payslips
- Download payslip PDFs
- Send payslip emails
- Track delivery history
- Retry failed delivery
- Secure sensitive payslip access

---

## Domain Model

### PayslipDocument
Generated PDF metadata.

### PayslipDeliveryLog
Email delivery attempts and results.

---

## Workflow

```text
Finalized Payroll
    ↓
Generated Payslip
    ↓
Generate PDF
    ↓
Review / Release
    ↓
Send Email
```

---

## Business Rules / Architecture Notes

- Follow Clean Architecture and CQRS + MediatR.
- Keep domain rules in the domain/application layer, not in controllers.
- Preserve historical records where the data can affect payroll, compliance, or employee history.
- Use explicit permissions for sensitive operations.
- Add auditability for state-changing actions.
- PDF must be generated from Payslip snapshot only.
- PDF generation must not recalculate payroll.

---

## Commands

- GeneratePayslipPdfCommand
- GeneratePayslipPdfsForPayrollRunCommand
- SendPayslipEmailCommand
- RetryPayslipEmailCommand

---

## Queries

- GetPayslipDocumentQuery
- GetPayslipDeliveryLogsQuery

---

## Permissions

```text
hr.payslip.view
hr.payslip.download
hr.payslip.send
```

---

## API Endpoints

```text
POST   /api/hr/payslips/{id}/pdf
GET    /api/hr/payslips/{id}/pdf
POST   /api/hr/payroll-runs/{id}/payslips/pdf
POST   /api/hr/payslips/{id}/send-email
GET    /api/hr/payslips/{id}/delivery-logs
```

---

## Frontend

- Download Payslip PDF
- Generate PDF for Payroll Run
- Send Payslip Email
- Delivery Status Table

---

## Future Enhancements

- Password-protected PDF
- Digital signature
- ESS PDF download
- Email template editor

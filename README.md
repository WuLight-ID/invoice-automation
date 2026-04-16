# Invoice Automation & BPM Validation

This project started as a simple idea: reduce the amount of manual work in handling invoices and approval flows.

It processes invoice data, validates form inputs, and integrates with a BPM system for approval workflows.

## What it does

- Extracts and maps invoice data
- Validates form inputs before submission (including BPM approval checks)
- Prevents invalid approvals (e.g. NG cases)
- Connects frontend (Angular) with backend services (C#)

## Tech stack

- Frontend: Angular
- Backend: C# (.NET)
- Database: SQL Server
- Other: JSON-based validation, BPM integration

## Why I built this

During development, I noticed a lot of repetitive manual checks in approval flows.  
Small mistakes (like approving NG cases) could easily slip through.

So I built a validation layer to catch these issues automatically before submission.

## Current status

Still improving:
- Better validation logic
- Cleaner API structure
- More reusable components

## Notes

This is part of my learning journey in building real-world systems, not just small demos.

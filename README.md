# DocProjDEVPLANT

## Overview

The project simplifies the process of creating and completing documents collaboratively. It allows users to create templates in DOCX format with placeholders (denoted by {{}}) for variables that need to be filled. Once templates are set up, users can either complete them themselves or invite others via email to fill out the required information.

The website for the project can be found <a href="https://team3.gdscupt.tech">here</a>, and the frontend of this project can be found <a href="https://github.com/RaulCandrea/DevPlantFrontend">here</a>

## Getting Started

To get started with the project, follow these steps:

1. <b>Account Setup:</b>
- If you're a new user, sign up for an account. If you've been invited to join, follow the link provided in the invitation email.

2. <b>Creating Templates:</b>
- Navigate to the Templates section, from the company you are assigned.
- Upload a DOCX file that includes placeholders ({{}}) where user input is required.

3. <b>Completing Documents:</b>
- Once a template is created, you can either:
- - Complete the document yourself by filling in the required information directly.
  - Send an email invitation to others to complete the document.

 4. <b>Guest Completion Process:</b>
 - Recipients invited via email do not need an account.
 - Upon clicking the link in the email, they will be directed to complete the required form.
 - After submission, a registration email will be sent to guests who do not have an account, inviting them to sign up for ongoing access.

5. <b>Document Finalization:</b>
- Once all required users have completed their parts, the document will be automatically generated.
- The generated document can be edited further if needed.
- For users with accounts, completed data is securely stored and can be automatically populated into future documents.

6. <b>Distribution:</b>
- The completed document will be sent via email to all participants involved in its creation.

## Features

- **Template Management:** Upload and manage DOCX templates.
- **Collaborative Editing:** Invite others to fill out documents via email.
- **Automatic Document Generation:** Once all required inputs are provided, documents are automatically compiled and distributed.
- **Data Autocompletion:** User data is stored and can be reused for future documents.
- **Document Editing:** Generated documents can be edited post-generation.
- **Profile Management:** Edit your personal information for future autocompletion, or upload an image of your **ID** to auto-complete relevant details.
- **User Management:** View all users in the company, their roles, and email addresses.

## Technologies used
- **.NET 8**
- **Entity Framework**
- **PostgreSQL**
- **Docker**
- **Kubernetes**
- **Firebase**
- **LibreOffice** (for converting a DOCX to PDF)
- **MinIO** (for storing the documents)

## Example Template
<details>
<summary>One user template with placeholders: </summary>
  
```
Car Rental Agreement

This Car Rental Agreement is made and entered into as of {{date.value}} by and between {{rental_company.name}} hereinafter referred to as "Lessor" and {{client.name}} hereinafter referred to as "Lessee."

1. Vehicle Description
The Lessor agrees to rent to the Lessee a vehicle described as follows:
- Make and Model: {{vehicle.model}}
- Year: {{vehicle.year}}
- Color: {{vehicle.color}}
- License Plate Number: {{vehicle.license_plate_number}}
- VIN: {{vehicle.identification_number}}

2. Rental Period
The rental period shall commence on {{rental.start_date}} and end on {{rental.end_date}}.

3. Rental Charges
The Lessee agrees to pay the Lessor the rental fee of {{rental.fee}} per day/week/month totaling {{rental.total_fee}} for the entire rental period.

4. Deposit
The Lessee agrees to pay a security deposit of {{security.deposit_amount}} which will be refunded upon return of the vehicle in the same condition as at the commencement of the rental period subject to any deductions for damages or additional charges.

5. Insurance
The Lessee is required to have insurance coverage. Details of the insurance policy are as follows:
- Insurance Provider: {{insurance_provider.name}}
- Policy Number: {{insurance_policy.number}}

6. Use of Vehicle
The Lessee agrees to the following terms regarding the use of the vehicle:
- The vehicle shall not be used for illegal purposes.

7. Maintenance and Repairs
The Lessee shall be responsible for routine maintenance of the vehicle including oil changes and tire pressure checks. Any necessary repairs during the rental period must be approved by the Lessor.

8. Return of Vehicle
The Lessee agrees to return the vehicle to the Lessor on {{rental.end_date}} in the same condition as at the commencement of the rental period subject to ordinary wear and tear.

9. Governing Law
This Agreement shall be governed by and construed in accordance with the laws of {{governing_law.location}}.

10. Signatures
By signing below both parties agree to the terms and conditions of this Car Rental Agreement.
- Lessor:
  - Name: {{rental_company.name}}
  - Signature: ______________________
  - Date: {{date.value}}
- Lessee:
  - Name: {{client.name}}
  - Signature: ______________________
  - Date: {{date.value}}
```
</details>

<details>
<summary>Two users template with placeholders: </summary>
  
```
Car Rental Agreement

This Car Rental Agreement is made and entered into as of {{date.value}} by and between {{rental_company.name}} hereinafter referred to as "Lessor" and {{client1.name}} and {{client2.name}} hereinafter referred to as "Lessee."

1. Vehicle Description
The Lessor agrees to rent to the Lessee a vehicle described as follows:
- Make and Model: {{vehicle.model}}
- Year: {{vehicle.year}}
- Color: {{vehicle.color}}
- License Plate Number: {{vehicle.license_plate_number}}
- VIN: {{vehicle.identification_number}}

2. Rental Period
The rental period shall commence on {{rental.start_date}} and end on {{rental.end_date}}.

3. Rental Charges
The Lessee agrees to pay the Lessor the rental fee of {{rental.fee}} per day/week/month totaling {{rental.total_fee}} for the entire rental period.

4. Deposit
The Lessee agrees to pay a security deposit of {{security.deposit_amount}} which will be refunded upon return of the vehicle in the same condition as at the commencement of the rental period subject to any deductions for damages or additional charges.

5. Insurance
The Lessee is required to have insurance coverage. Details of the insurance policy are as follows:
- Insurance Provider: {{insurance_provider.name}}
- Policy Number: {{insurance_policy.number}}

6. Use of Vehicle
The Lessee agrees to the following terms regarding the use of the vehicle:
- The vehicle shall not be used for illegal purposes.

7. Maintenance and Repairs
The Lessee shall be responsible for routine maintenance of the vehicle including oil changes and tire pressure checks. Any necessary repairs during the rental period must be approved by the Lessor.

8. Return of Vehicle
The Lessee agrees to return the vehicle to the Lessor on {{rental.end_date}} in the same condition as at the commencement of the rental period subject to ordinary wear and tear.

9. Governing Law
This Agreement shall be governed by and construed in accordance with the laws of {{governing_law.location}}.

10. Signatures
By signing below both parties agree to the terms and conditions of this Car Rental Agreement.
- Lessor:
  - Name: {{rental_company.name}}
  - Signature: ______________________
  - Date: {{date.value}}
- Lessee 1:
  - Name: {{client1.name}}
  - Signature: ______________________
  - Date: {{date.value}}
- Lessee 2:
  - Name: {{client2.name}}
  - Signature: ______________________
  - Date: {{date.value}}

```
</details>


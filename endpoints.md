# API Endpoints: Delivery & Revision Integration Guide

This document details the exact API endpoints, request schemas, and response payloads for integrating the **Delivery & Revision lifecycle** in the frontend.

---

## 1. Upload Delivery Files (Freelancer)
Used by the freelancer to physically upload work files before formal submission.

* **HTTP Method**: `POST`
* **Route**: `/api/deliveries/upload`
* **Auth**: Required (`Freelancer` role)
* **Request Format**: `multipart/form-data`
  - `files` (Array of files to upload)
* **Response Body** (`List<AttachmentDto>`):
  ```json
  [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "fileUrl": "/uploads/deliveries/draft_v1.zip",
      "fileName": "draft_v1.zip",
      "originalFileName": "draft_v1.zip",
      "fileType": "application/zip",
      "fileSizeBytes": 456000,
      "uploadedAt": "2026-06-20T12:00:00Z"
    }
  ]
  ```

---

## 2. Submit Delivery Attempt (Freelancer)
Used by the freelancer to submit a new work delivery attempt with attached files.

* **HTTP Method**: `POST`
* **Route**: `/api/deliveries/submit`
* **Auth**: Required (`Freelancer` role)
* **Request Body** (`SubmitDeliveryRequest`):
  ```json
  {
    "contractId": 42,
    "contractMilestoneId": "8f31b8a5-dcd7-4d7a-b924-f7b6d1945a0b",
    "deliveryNote": "Initial deployment build and files.",
    "attachments": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fileUrl": "/uploads/deliveries/draft_v1.zip",
        "originalFileName": "draft_v1.zip",
        "fileType": "application/zip",
        "fileSizeBytes": 456000,
        "uploadedAt": "2026-06-20T12:00:00Z"
      }
    ]
  }
  ```
* **Response Body** (`ContractDeliveryDto`):
  ```json
  {
    "id": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
    "contractId": 42,
    "contractMilestoneId": "8f31b8a5-dcd7-4d7a-b924-f7b6d1945a0b",
    "submittedAt": "2026-06-20T12:05:00Z",
    "deliveryNote": "Initial deployment build and files.",
    "status": "Pending", // Pending, Approved, Rejected, UnderReview, RevisionRequested, Disputed
    "reviewDeadline": "2026-06-23T12:05:00Z",
    "completedAt": null,
    "attachments": [
      {
        "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        "fileUrl": "/uploads/deliveries/draft_v1.zip",
        "originalFileName": "draft_v1.zip",
        "fileType": "application/zip",
        "fileSizeBytes": 456000
      }
    ],
    "isPaused": false,
    "pauseReason": null
  }
  ```

---

## 3. Get Contract Deliveries (Client & Freelancer)
Used by both the client and freelancer to retrieve all delivery attempts made on a contract.

* **HTTP Method**: `GET`
* **Route**: `/api/deliveries?contractId=42`
* **Auth**: Required
* **Response Body** (`List<ContractDeliveryDto>`):
  ```json
  [
    {
      "id": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
      "contractId": 42,
      "contractMilestoneId": "8f31b8a5-dcd7-4d7a-b924-f7b6d1945a0b",
      "submittedAt": "2026-06-20T12:05:00Z",
      "deliveryNote": "Initial deployment build and files.",
      "status": "RevisionRequested",
      "reviewDeadline": "2026-06-23T12:05:00Z",
      "completedAt": null,
      "attachments": [
        {
          "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
          "fileUrl": "/uploads/deliveries/draft_v1.zip",
          "originalFileName": "draft_v1.zip"
        }
      ],
      "isPaused": true,
      "pauseReason": "RevisionRequest"
    }
  ]
  ```

---

## 4. Download Delivery Attachment (Client & Freelancer)
Used to download a specific delivery file attachment.

* **HTTP Method**: `GET`
* **Route**: `/api/deliveries/attachments/{attachmentId}/download`
* **Auth**: Required
* **Response**: Physical file stream (`application/octet-stream` or matching content-type)

---

## 5. Request Direct Revision (Client)
Used by the client to request a revision for a pending delivery. Only allowed if `MaxRevisions` has not been exceeded.

* **HTTP Method**: `POST`
* **Route**: `/api/deliveries/{deliveryId}/revision`
* **Auth**: Required (`Client` role)
* **Request Body**:
  ```json
  {
    "reason": "Please change the header color to blue and align the logo."
  }
  ```
* **Success Response (201 Created)** (`RevisionRequestDto`):
  ```json
  {
    "id": "7ca6412f-6821-4bde-8f81-2bb45df7456d",
    "deliveryId": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
    "requestedByClientId": "client-user-uuid",
    "reason": "Please change the header color to blue and align the logo.",
    "requestedAt": "2026-06-20T12:15:00Z",
    "status": "Pending", // Pending, AcceptedBySpecialist, Resolved
    "specialistId": null,
    "specialistDecision": null,
    "resolvedAt": null
  }
  ```
* **Failure Response (400 Bad Request - Revision limit exceeded)**:
  ```json
  {
    "errorCode": "REVISION_LIMIT_EXCEEDED",
    "message": "You have reached the maximum allowed revisions (3) for this contract. Please request additional revisions from the freelancer."
  }
  ```

---

## 6. Retrieve Freelancer Revision Requests (Freelancer)
Used by the freelancer to retrieve historical and pending revision requests (with reason notes) for their contracts.

* **HTTP Method**: `GET`
* **Route**: `/api/revisions/freelancer?contractId=42`
* **Auth**: Required (`Freelancer` role)
* **Response Body** (`Result<List<RevisionRequestDto>>`):
  ```json
  {
    "succeeded": true,
    "message": "",
    "errorCode": "",
    "errors": [],
    "data": [
      {
        "id": "7ca6412f-6821-4bde-8f81-2bb45df7456d",
        "deliveryId": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
        "requestedByClientId": "client-user-uuid",
        "reason": "Please change the header color to blue and align the logo.",
        "requestedAt": "2026-06-20T12:15:00Z",
        "status": "Resolved", // Resolves automatically upon freelancer's new delivery submission
        "specialistId": null,
        "specialistDecision": null,
        "resolvedAt": "2026-06-20T12:30:00Z"
      }
    ]
  }
  ```

---

## 7. Request Additional Revisions (Client)
Used by the client to request more revision limits when the contract limit is exceeded.

* **HTTP Method**: `POST`
* **Route**: `/api/revisions/additional/request`
* **Auth**: Required (`Client` role)
* **Request Body**:
  ```json
  {
    "deliveryId": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
    "requestedCount": 2,
    "reason": "Need a couple more adjustments to finish the final styling."
  }
  ```
* **Response Body** (`AdditionalRevisionRequestDto`):
  ```json
  {
    "id": "5fa23d47-68b3-4f9e-ad34-bc2c8e312a0d",
    "deliveryId": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
    "clientId": "client-user-uuid",
    "requestedCount": 2,
    "reason": "Need a couple more adjustments to finish the final styling.",
    "status": "Pending", // Pending, Approved, Rejected
    "requestedAt": "2026-06-20T12:45:00Z",
    "resolvedAt": null
  }
  ```

---

## 8. View Pending Additional Revisions (Freelancer)
Used by the freelancer to check requested extra revision counts.

* **HTTP Method**: `GET`
* **Route**: `/api/revisions/additional/pending`
* **Auth**: Required (`Freelancer` role)
* **Response Body** (`Result<List<AdditionalRevisionRequestDto>>`):
  ```json
  {
    "succeeded": true,
    "message": "",
    "errorCode": "",
    "errors": [],
    "data": [
      {
        "id": "5fa23d47-68b3-4f9e-ad34-bc2c8e312a0d",
        "deliveryId": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
        "clientId": "client-user-uuid",
        "requestedCount": 2,
        "reason": "Need a couple more adjustments to finish the final styling.",
        "status": "Pending",
        "requestedAt": "2026-06-20T12:45:00Z"
      }
    ]
  }
  ```

---

## 9. Respond to Additional Revisions Request (Freelancer)
Used by the freelancer to accept or decline the client's request for additional revisions.

* **HTTP Method**: `POST`
* **Route**: `/api/revisions/additional/{requestId}/respond`
* **Auth**: Required (`Freelancer` role)
* **Request Body**:
  ```json
  {
    "accept": true
  }
  ```
* **Response Body** (`Result<bool>`):
  ```json
  {
    "succeeded": true,
    "message": "Additional revision request accepted.",
    "data": true
  }
  ```

---

## 10. Approve Delivery (Client)
Used by the client to approve the final delivery and release escrowed funds.

* **HTTP Method**: `POST`
* **Route**: `/api/deliveries/{deliveryId}/approve`
* **Auth**: Required (`Client` role)
* **Response Body** (`ContractDeliveryDto`):
  ```json
  {
    "id": "9a38f32c-3965-4f40-8b17-09d6f6e520a7",
    "contractId": 42,
    "status": "Approved",
    "completedAt": "2026-06-20T13:00:00Z",
    "attachments": [...]
  }
  ```

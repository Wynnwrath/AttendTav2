---
id: 7
type: story
title: "Transactional certificate issuance pipeline and audit"
parent: epic-certificates
covers: [R3, R5, R7, R8]
after: [2, 6, 1.3, 1.6, 1.9, 2.2]
hitl: false
risk: high
tracker_id: "67"
remote: "https://github.com/rainyDayssss/AttendTa/issues/67"
tracker_status: backlog
---

# Transactional certificate issuance pipeline and audit

## Description

Delivers the issuance use case for R5 and the paged issuance queue for R8 (AD-13) in this order: pre-checks and the existing issued state are read outside any lock; the fixed template renders to bytes with a unique certificate number (format decided by this story, Deferred); the bytes are saved to the temporary area and promoted to certificates/ through IPrivateFileStore, with server-generated names opened CreateNew; then one ExecuteInTransactionAsync locks the owner's users row, the event and the review through CertificateRepository.LockCertificateAsync(id), which this entry adds (AD-4 step 7), rechecks approval, Valid Attendance and Completed status, and sets certificate_number, the snapshots, issuer_name_snapshot, file_path (relative, certificates/…), issued_by and issued_at, appending a certificate.issued entry whose values exclude file_path; if the transaction fails the promoted file is deleted inline (no background cleanup, AD-16), and a repeat issuance is refused as Duplicate without touching the issued PDF. The SM-4 concurrency tests in tests/ASI.Basecode.IntegrationTests (AD-17) run parallel issuance and invalidation against real locks, with the renderer replaced by a test double that returns fixed valid PDF bytes, since the CI workflow from epic-platform-access (entry 10) has no Chromium, delivering R3 (the invalidation-versus-issuance race), R5 (issuance), R7 (the certificate.issued audit entry) and R8 (the issuance queue).

## Requirements

Verify: An Officer issues an eligible certificate from the paged queue, and the stored PDF is a relative certificates/ path whose bytes start with %PDF-, with unique certificate_number, issuer_name_snapshot and issued_at recorded; a repeat issuance is refused as Duplicate and leaves the stored file unchanged; SQL Server integration tests show parallel issuance yields exactly one issued certificate, and parallel invalidation and issuance each resolve to exactly one outcome (the invalidation is refused and the certificate is issued, or the certificate is not issued and the review is not_eligible).

## References

- parent — _bmad-output/initiative-attendta/tickets/epic-certificates/epic-certificates.md

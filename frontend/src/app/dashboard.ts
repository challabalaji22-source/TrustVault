
import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpHeaders } from '@angular/common/http';

interface AuditItem {
  id: number;
  userId: string | null;
  action: string;
  documentId: string | null;
  fileName: string;
  details: string;
  succeeded: boolean;
  occurredAt: string;
}

interface DocumentItem {
  id: string;
  fileName: string;
  category: string;
  sizeBytes: number;
  uploadedAt: string;
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <main class="dashboard">
      <header class="dashboard-header">
        <div>
          <h1>Welcome to TrustVault</h1>
          <p>Your secure document workspace</p>
        </div>
        <button (click)="logout()">Sign out</button>
      </header>

      <section>
        <h2>Audit History</h2>

        <p *ngIf="auditLoading">Loading audit history...</p>

        <p *ngIf="!auditLoading && auditEvents.length === 0">
          No audit history available yet.
        </p>

        <p *ngIf="!auditLoading && auditEvents.length > 0">
          Audit records received: {{ auditEvents.length }}
        </p>

        <div class="document" *ngFor="let event of auditEvents">
          <strong>{{ event.action }}</strong>
          <p>File: {{ event.fileName || 'Unavailable' }}</p>
          <p>Details: {{ event.details }}</p>
          <p>Status: {{ event.succeeded ? 'Successful' : 'Failed' }}</p>
          <p>Time: {{ event.occurredAt | date:'medium' }}</p>
        </div>

        <button (click)="loadAuditHistory()" [disabled]="auditLoading">
          Refresh audit history
        </button>
      </section>

      <section>
      <section>
  <h2>Shared With Me</h2>

  <p *ngIf="sharedDocuments.length === 0">
    No documents have been shared with you yet.
  </p>

  <div class="document" *ngFor="let doc of sharedDocuments">
    <strong>{{ doc.fileName }}</strong>
    <p>Category: {{ doc.category || 'Uncategorized' }}</p>
    <p>Size: {{ doc.sizeBytes }} bytes</p>

    <button (click)="downloadDocument(doc)">
      Download
    </button>
  </div>
</section>
        <h2>Your Documents</h2>

        <input
          type="text"
          placeholder="Search documents..."
          #searchBox
          (input)="searchTerm = searchBox.value"
        />

        <p *ngIf="loading">Loading documents...</p>

        <p *ngIf="message" role="status">{{ message }}</p>

        <div class="document" *ngFor="let doc of filteredDocuments">
          <strong>{{ doc.fileName }}</strong>
          <p>Category: {{ doc.category || 'Uncategorized' }}</p>
          <p>Size: {{ doc.sizeBytes }} bytes</p>
          <p>Uploaded: {{ doc.uploadedAt | date:'medium' }}</p>

          <button (click)="downloadDocument(doc)">Download</button>
          <button (click)="shareDocument(doc)">Share</button>
        </div>

        <p *ngIf="!loading && documents.length === 0">
          No documents found. Upload a document to get started.
        </p>

        <div class="upload-controls">
          <input
            type="file"
            #fileInput
            (change)="uploadDocument($event)"
          />

          <button (click)="loadDocuments()" [disabled]="loading">
            Refresh documents
          </button>
        </div>
      </section>
    </main>
  `,
  styles: [`
    .dashboard {
      max-width: 900px;
      margin: 40px auto;
      padding: 24px;
      font-family: Arial, sans-serif;
      color: #222;
    }

    .dashboard-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      gap: 16px;
      margin-bottom: 28px;
    }

    section {
      margin-bottom: 32px;
    }

    .document {
      border: 1px solid #ddd;
      border-radius: 8px;
      padding: 16px;
      margin: 12px 0;
      overflow-wrap: anywhere;
    }

    .document p {
      margin: 6px 0;
    }

    input[type="text"] {
      width: 100%;
      max-width: 400px;
      box-sizing: border-box;
      padding: 10px;
      margin-bottom: 12px;
    }

    button {
      padding: 10px 16px;
      margin: 4px;
      cursor: pointer;
    }

    button:disabled {
      cursor: not-allowed;
      opacity: 0.6;
    }

    .upload-controls {
      margin-top: 16px;
    }

    @media (max-width: 600px) {
      .dashboard {
        margin: 12px auto;
        padding: 14px;
      }

      .dashboard-header {
        align-items: flex-start;
      }
    }
  `]
})
export class Dashboard implements OnInit {
  documents: DocumentItem[] = [];
  sharedDocuments: DocumentItem[] = [];
  auditEvents: AuditItem[] = [];

  loading = false;
  auditLoading = false;
  message = '';
  searchTerm = '';

  constructor(
    private http: HttpClient,
    private cdr: ChangeDetectorRef
  ) {}

  get filteredDocuments(): DocumentItem[] {
    const term = this.searchTerm.trim().toLowerCase();

    if (!term) {
      return this.documents;
    }

    return this.documents.filter(doc =>
      doc.fileName.toLowerCase().includes(term) ||
      (doc.category || '').toLowerCase().includes(term)
    );
  }

  ngOnInit(): void {
    if (!sessionStorage.getItem('trustvault_token')) {
      window.location.href = '/login';
      return;
    }

    this.loadDocuments();
    this.loadSharedDocuments();
    this.loadAuditHistory();
  }

  private getAuthHeaders(): HttpHeaders | null {
    const token = sessionStorage.getItem('trustvault_token');

    if (!token) {
      return null;
    }

    return new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
  }

  loadDocuments(): void {
    const headers = this.getAuthHeaders();

    if (!headers) {
      this.message = 'Please sign in again to view your documents.';
      this.loading = false;
      return;
    }

    this.loading = true;
    this.message = '';

    this.http.get<DocumentItem[]>(
      'http://localhost:5201/api/documents',
      { headers }
    ).subscribe({
      next: data => {
        this.documents = data;
        this.loading = false;
        this.cdr.detectChanges();
        console.log('Documents loaded:', data.length);
      },
      error: error => {
        this.loading = false;
        this.message =
          `Could not load documents (HTTP ${error.status || 'connection error'}).`;
        console.error('Document loading failed:', error);
        this.cdr.detectChanges();
      }
    });
  }
  loadSharedDocuments(): void {
  const headers = this.getAuthHeaders();

  if (!headers) {
    this.sharedDocuments = [];
    return;
  }

  this.http.get<DocumentItem[]>(
    'http://localhost:5201/api/documents/shared',
    { headers }
  ).subscribe({
    next: (data) => {
      this.sharedDocuments = data;
      console.log('Shared documents loaded:', data);
      this.cdr.detectChanges();
    },
    error: (error) => {
      console.error('Shared documents loading failed:', error);
    }
  });
}

  loadAuditHistory(): void {
    const headers = this.getAuthHeaders();

    if (!headers) {
      this.auditEvents = [];
      this.auditLoading = false;
      return;
    }

    this.auditLoading = true;

    this.http.get<AuditItem[]>(
      'http://localhost:5201/api/documents/audit',
      { headers }
    ).subscribe({
      next: data => {
        this.auditEvents = data;
        this.auditLoading = false;
        console.log('Audit events loaded:', data);
        this.cdr.detectChanges();
      },
      error: error => {
        this.auditLoading = false;
        console.error('Audit history loading failed:', error);
        this.cdr.detectChanges();
      }
    });
  }

  uploadDocument(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];

    if (!file) return;

    const headers = this.getAuthHeaders();

    if (!headers) {
      this.message = 'Please sign in again.';
      return;
    }

    if (file.size > 10 * 1024 * 1024) {
      this.message = 'File must be 10 MB or smaller.';
      input.value = '';
      return;
    }

    const formData = new FormData();
    formData.append('file', file);

    this.loading = true;
    this.message = '';

    this.http.post(
      'http://localhost:5201/api/documents/upload',
      formData,
      { headers, params: { category: 'General' } }
    ).subscribe({
      next: () => {
        this.loading = false;
        this.message = 'Document uploaded successfully!';
        input.value = '';
        this.loadDocuments();
        this.loadAuditHistory();
        this.cdr.detectChanges();
      },
      error: error => {
        this.loading = false;
        this.message =
          `Upload failed (HTTP ${error.status || 'connection error'}).`;
        console.error('Upload failed:', error);
        this.cdr.detectChanges();
      }
    });
  }

  downloadDocument(doc: DocumentItem): void {
    const headers = this.getAuthHeaders();

    if (!headers) {
      this.message = 'Please sign in again.';
      return;
    }

    this.http.get(
      `http://localhost:5201/api/documents/${doc.id}/download`,
      { headers, responseType: 'blob' }
    ).subscribe({
      next: blob => {
        const url = URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = doc.fileName;
        link.click();
        URL.revokeObjectURL(url);

        this.message = 'Download started.';
        this.loadAuditHistory();
      },
      error: error => {
        this.message =
          `Download failed (HTTP ${error.status || 'connection error'}).`;
        console.error('Download failed:', error);
        this.loadAuditHistory();
      }
    });
  }

  shareDocument(doc: DocumentItem): void {
    const email = prompt(
      'Enter the email address of the person to share with:'
    );

    if (!email || !email.trim()) return;

    const headers = this.getAuthHeaders();

    if (!headers) {
      this.message = 'Please sign in again.';
      return;
    }

    this.http.post(
      `http://localhost:5201/api/documents/${doc.id}/permissions`,
      null,
      {
        headers,
        params: {
          email: email.trim(),
          canRead: 'true',
          canEdit: 'false'
        }
      }
    ).subscribe({
      next: () => {
        this.message =
          `Document shared with ${email.trim()} successfully.`;
        this.loadAuditHistory();
      },
      error: error => {
        this.message =
          `Sharing failed (HTTP ${error.status || 'connection error'}).`;
        console.error('Sharing failed:', error);
      }
    });
  }

  logout(): void {
    sessionStorage.removeItem('trustvault_token');
    window.location.href = '/login';
  }
}

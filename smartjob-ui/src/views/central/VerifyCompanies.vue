<template>
  <div class="central-verify-page">
    <div class="page-header">
      <div class="header-left">
        <h2>Verify Companies</h2>
        <p class="text-muted small ms-3 mb-0">Review and verify company registration documents.</p>
      </div>
    </div>

    <!-- COMPANIES LIST -->
    <div class="card-section">
      <div class="table-wrapper">
        <table class="app-table">
          <thead>
            <tr>
              <th>Company Name</th>
              <th>Industry</th>
              <th>Location</th>
              <th>Status</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="company in companies" :key="company.companyId">
              <td class="fw">{{ company.companyName }}</td>
              <td>{{ company.industry }}</td>
              <td>{{ company.location }}</td>
              <td>
                <span :class="['status-badge', company.isCompanyVerified ? 'active' : 'pending']">
                  {{ company.isCompanyVerified ? 'Verified' : 'Pending' }}
                </span>
              </td>
              <td>
                <button class="action-btn view-btn" @click="openDocsModal(company)">
                  <i class="bi bi-file-earmark-text"></i> Review Docs
                </button>
              </td>
            </tr>
            <tr v-if="companies.length === 0">
              <td colspan="5" class="no-data py-5">
                <div class="text-center text-muted">
                  <i class="bi bi-building fs-1 mb-2"></i>
                  <p>No companies found.</p>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- DOCUMENTS REVIEW MODAL -->
    <div class="custom-modal-overlay" v-if="showDocsModal" @click.self="closeDocsModal">
      <div class="custom-modal docs-modal">
        <div class="modal-header">
          <h3>Review: {{ selectedCompany?.companyName }}</h3>
          <button class="close-btn" @click="closeDocsModal"><i class="bi bi-x-lg"></i></button>
        </div>
        <div class="modal-body">
          <div v-if="loadingDocs" class="text-center py-4">
            <div class="spinner-border text-primary"></div>
            <p class="mt-2">Fetching documents...</p>
          </div>
          
          <div v-else-if="documents.length > 0">
            <h5 class="mb-3">Submitted Documents</h5>
            <div class="list-group mb-4">
              <div v-for="doc in documents" :key="doc.documentId" class="list-group-item d-flex justify-content-between align-items-center">
                <div>
                  <h6 class="mb-0">{{ doc.documentType }}</h6>
                  <small class="text-muted">{{ doc.fileName }}</small>
                </div>
                <a :href="`${API_HOST}${doc.filePath}`" target="_blank" class="btn btn-sm btn-outline-primary">
                  <i class="bi bi-eye"></i> View
                </a>
              </div>
            </div>

            <hr />

            <div v-if="!selectedCompany?.isCompanyVerified">
              <h5 class="mb-3">Verification Action</h5>
              <div class="mb-3">
                <label class="form-label">Rejection Reason (if rejecting)</label>
                <textarea v-model="verifyReason" class="form-control" rows="2" placeholder="Explain why documents are rejected..."></textarea>
              </div>
              <div class="d-flex gap-2">
                <button class="btn btn-success flex-grow-1" @click="handleVerify(true)" :disabled="verifying">
                  Approve Company
                </button>
                <button class="btn btn-danger flex-grow-1" @click="handleVerify(false)" :disabled="verifying || !verifyReason">
                  Reject
                </button>
              </div>
            </div>
            <div v-else class="alert alert-success mt-3 mb-0">
              <i class="bi bi-check-circle-fill me-2"></i> This company is already verified.
            </div>
          </div>

          <div v-else class="text-center py-4">
            <i class="bi bi-exclamation-triangle-fill text-warning fs-3 mb-2"></i>
            <p>No documents found for this company.</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from "vue";
import { getCentralCompanies, getCompanyVerificationDocuments, verifyCompany, API_HOST } from "@/services/api";
import { useNotification } from "@/composables/useNotification";

const { notify } = useNotification();
const companies = ref([]);
const documents = ref([]);
const selectedCompany = ref(null);

const showDocsModal = ref(false);
const loadingDocs = ref(false);
const verifying = ref(false);
const verifyReason = ref("");

const loadCompanies = async () => {
  try {
    companies.value = await getCentralCompanies();
  } catch (err) {
    notify("Error loading companies: " + err.message, "error");
  }
};

const openDocsModal = async (company) => {
  selectedCompany.value = company;
  showDocsModal.value = true;
  loadingDocs.value = true;
  documents.value = [];
  verifyReason.value = "";

  try {
    documents.value = await getCompanyVerificationDocuments(company.companyId);
  } catch (err) {
    notify("Error loading documents: " + err.message, "error");
  } finally {
    loadingDocs.value = false;
  }
};

const closeDocsModal = () => {
  showDocsModal.value = false;
  selectedCompany.value = null;
};

const handleVerify = async (isApproved) => {
  if (!selectedCompany.value) return;

  verifying.value = true;
  try {
    const dto = {
      companyId: selectedCompany.value.companyId,
      isApproved,
      reason: isApproved ? "Approved by Admin" : verifyReason.value
    };
    await verifyCompany(dto);
    notify(isApproved ? "Company approved!" : "Company rejected.", "success");
    loadCompanies();
    closeDocsModal();
  } catch (err) {
    notify("Action failed: " + err.message, "error");
  } finally {
    verifying.value = false;
  }
};

onMounted(loadCompanies);
</script>

<style scoped>
.central-verify-page {
  padding: 30px;
}
.page-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 25px;
}
.card-section {
  background: white;
  padding: 20px;
  border-radius: 12px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.05);
}
.app-table {
  width: 100%;
  border-collapse: collapse;
}
.app-table th, .app-table td {
  padding: 12px 15px;
  text-align: left;
  border-bottom: 1px solid #eee;
}
.status-badge {
  padding: 4px 10px;
  border-radius: 20px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}
.active { background: #d1fae5; color: #065f46; }
.pending { background: #fef3c7; color: #92400e; }

.custom-modal-overlay {
  position: fixed;
  top: 0; left: 0; width: 100%; height: 100%;
  background: rgba(0,0,0,0.5);
  display: flex; justify-content: center; align-items: center;
  z-index: 1000;
}
.custom-modal {
  background: white;
  width: 90%;
  max-width: 500px;
  border-radius: 15px;
  overflow: hidden;
}
.modal-header {
  padding: 15px 20px;
  border-bottom: 1px solid #eee;
  display: flex; justify-content: space-between; align-items: center;
}
.modal-body { padding: 20px; }
.close-btn { background: none; border: none; font-size: 20px; cursor: pointer; }

.action-btn {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 13px;
  border: none;
  cursor: pointer;
}
.view-btn { background: #e0f2fe; color: #0369a1; }
</style>

<template>
  <div class="container-fluid py-4">
    <div class="row justify-content-center">
      <div class="col-lg-8">
        <div class="card border-0 shadow-sm rounded-4 overflow-hidden">
          <div class="card-header bg-white border-bottom py-3">
            <h5 class="m-0 fw-bold d-flex align-items-center">
              <ShieldCheck class="me-2 text-primary" />
              Company Verification
            </h5>
          </div>
          <div class="card-body p-4">
            
            <!-- Status Badge -->
            <div class="mb-4 d-flex align-items-center justify-content-between p-3 rounded-3" :class="statusBgClass">
              <div class="d-flex align-items-center">
                <div class="status-icon me-3">
                  <component :is="statusIcon" size="24" />
                </div>
                <div>
                  <h6 class="m-0 fw-bold">{{ statusText }}</h6>
                  <p class="m-0 small text-opacity-75">Verification Status</p>
                </div>
              </div>
              <div v-if="isVerified" class="badge bg-success py-2 px-3 rounded-pill">Verified</div>
              <div v-else-if="isPending" class="badge bg-warning text-dark py-2 px-3 rounded-pill">Under Review</div>
              <div v-else-if="isRejected" class="badge bg-danger py-2 px-3 rounded-pill">Action Required</div>
              <div v-else class="badge bg-secondary py-2 px-3 rounded-pill">Not Submitted</div>
            </div>

            <!-- Rejection Reason -->
            <div v-if="rejectionReason" class="alert alert-danger border-0 rounded-3 mb-4 d-flex align-items-center">
              <AlertCircle class="me-2" size="20" />
              <div>
                <strong>Rejection Reason:</strong> {{ rejectionReason }}
              </div>
            </div>

            <div v-if="isVerified" class="text-center py-5">
              <div class="mb-3 text-success">
                <CheckCircle size="64" />
              </div>
              <h3>Your Company is Verified!</h3>
              <p class="text-muted">You have full access to all recruitment features including posting jobs and marking placements.</p>
              <router-link to="/company/jobs" class="btn btn-primary rounded-pill px-4 py-2 mt-3">
                Go to Posted Jobs
              </router-link>
            </div>

            <div v-else>
              <p class="text-muted mb-4">
                To comply with our safety standards, please upload the following documents for verification. 
                Our team will review your submission within 24-48 hours.
              </p>

              <form @submit.prevent="handleUpload" class="needs-validation">
                <!-- Incorporation -->
                <div class="mb-4">
                  <label class="form-label fw-bold d-flex align-items-center">
                    <FileText size="18" class="me-2 text-primary" />
                    Certificate of Incorporation
                  </label>
                  <div class="file-upload-box" :class="{ 'has-file': files.incorporation }">
                    <input type="file" @change="onFileChange($event, 'incorporation')" accept=".pdf,.jpg,.jpeg,.png" id="incorporation" class="d-none" />
                    <label for="incorporation" class="w-100 py-4 px-3 text-center cursor-pointer">
                      <div v-if="!files.incorporation">
                        <Upload class="mb-2 text-muted" />
                        <p class="m-0 small">Click to upload or drag & drop</p>
                        <p class="m-0 text-muted extra-small">PDF, JPG, PNG (Max 5MB)</p>
                      </div>
                      <div v-else class="d-flex align-items-center justify-content-center text-primary">
                        <FileCheck size="20" class="me-2" />
                        <span class="small fw-bold">{{ files.incorporation.name }}</span>
                      </div>
                    </label>
                  </div>
                </div>

                <!-- GST -->
                <div class="mb-4">
                  <label class="form-label fw-bold d-flex align-items-center">
                    <Hash size="18" class="me-2 text-primary" />
                    GST Registration Certificate
                  </label>
                  <div class="file-upload-box" :class="{ 'has-file': files.gst }">
                    <input type="file" @change="onFileChange($event, 'gst')" accept=".pdf,.jpg,.jpeg,.png" id="gst" class="d-none" />
                    <label for="gst" class="w-100 py-4 px-3 text-center cursor-pointer">
                      <div v-if="!files.gst">
                        <Upload class="mb-2 text-muted" />
                        <p class="m-0 small">Click to upload or drag & drop</p>
                      </div>
                      <div v-else class="d-flex align-items-center justify-content-center text-primary">
                        <FileCheck size="20" class="me-2" />
                        <span class="small fw-bold">{{ files.gst.name }}</span>
                      </div>
                    </label>
                  </div>
                </div>

                <!-- PAN -->
                <div class="mb-4">
                  <label class="form-label fw-bold d-flex align-items-center">
                    <CreditCard size="18" class="me-2 text-primary" />
                    Company PAN Card
                  </label>
                  <div class="file-upload-box" :class="{ 'has-file': files.pan }">
                    <input type="file" @change="onFileChange($event, 'pan')" accept=".pdf,.jpg,.jpeg,.png" id="pan" class="d-none" />
                    <label for="pan" class="w-100 py-4 px-3 text-center cursor-pointer">
                      <div v-if="!files.pan">
                        <Upload class="mb-2 text-muted" />
                        <p class="m-0 small">Click to upload or drag & drop</p>
                      </div>
                      <div v-else class="d-flex align-items-center justify-content-center text-primary">
                        <FileCheck size="20" class="me-2" />
                        <span class="small fw-bold">{{ files.pan.name }}</span>
                      </div>
                    </label>
                  </div>
                </div>

                <div class="mt-5">
                  <button type="submit" class="btn btn-primary w-100 py-3 rounded-3 fw-bold d-flex align-items-center justify-content-center" :disabled="isUploading || !isFormValid">
                    <span v-if="isUploading" class="spinner-border spinner-border-sm me-2"></span>
                    <Lock v-else size="18" class="me-2" />
                    {{ isUploading ? 'Uploading Documents...' : 'Submit Documents for Verification' }}
                  </button>
                  <p class="text-center mt-3 small text-muted">
                    Your documents are stored securely and used only for verification purposes.
                  </p>
                </div>
              </form>
            </div>

          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from "vue";
import { 
  ShieldCheck, 
  Upload, 
  FileCheck, 
  CheckCircle, 
  AlertCircle, 
  Clock, 
  FileText, 
  Hash, 
  CreditCard,
  Lock,
  ChevronRight
} from "lucide-vue-next";
import { 
  uploadVerificationDocuments, 
  getCompanyVerificationDocuments, 
  getSetupCompanies, 
  checkSession 
} from "@/services/api";
import { useNotification } from "@/composables/useNotification";

const { notify } = useNotification();

const files = ref({
  incorporation: null,
  gst: null,
  pan: null
});

const isUploading = ref(false);
const status = ref("none"); // none, pending, verified, rejected
const rejectionReason = ref("");

const isFormValid = computed(() => {
  return files.value.incorporation && files.value.gst && files.value.pan;
});

const isPending = computed(() => status.value === "pending");
const isVerified = computed(() => status.value === "verified");
const isRejected = computed(() => status.value === "rejected");

const statusText = computed(() => {
  if (isVerified.value) return "Verified";
  if (isPending.value) return "Pending Approval";
  if (isRejected.value) return "Rejected";
  return "Not Submitted";
});

const statusBgClass = computed(() => {
  if (isVerified.value) return "bg-success-soft text-success";
  if (isPending.value) return "bg-warning-soft text-warning-dark";
  if (isRejected.value) return "bg-danger-soft text-danger";
  return "bg-light text-muted";
});

const statusIcon = computed(() => {
  if (isVerified.value) return CheckCircle;
  if (isPending.value) return Clock;
  if (isRejected.value) return AlertCircle;
  return ShieldCheck;
});

function onFileChange(e, type) {
  const file = e.target.files[0];
  if (file) {
    files.value[type] = file;
  }
}

async function fetchStatus() {
  try {
    const userData = await checkSession();
    const companyId = userData.companyId;

    if (companyId) {
      const companies = await getSetupCompanies();
      const current = companies.find(c => c.companyId === companyId);
      
      if (current) {
        if (current.isCompanyVerified) {
          status.value = "verified";
        } else {
          try {
            const docs = await getCompanyVerificationDocuments(companyId);
            if (docs && docs.length > 0) {
              const anyRejected = docs.some(d => d.isRejected);
              if (anyRejected) {
                status.value = "rejected";
                rejectionReason.value = docs.find(d => d.isRejected)?.rejectReason || "Documents rejected by admin.";
              } else {
                status.value = "pending";
              }
            }
          } catch (docErr) {
            console.error("No documents found or fetch error", docErr);
          }
        }
      }
    }
  } catch (err) {
    console.error("Failed to fetch status", err);
  }
}

async function handleUpload() {
  isUploading.value = true;
  const formData = new FormData();
  formData.append("files", files.value.incorporation);
  formData.append("files", files.value.gst);
  formData.append("files", files.value.pan);

  try {
    await uploadVerificationDocuments(formData);
    notify("Documents uploaded successfully!", "success");
    status.value = "pending";
  } catch (err) {
    notify(err.message || "Upload failed", "error");
  } finally {
    isUploading.value = false;
  }
}

onMounted(() => {
  fetchStatus();
});
</script>

<style scoped>
.file-upload-box {
  border: 2px dashed #e2e8f0;
  border-radius: 12px;
  background: #f8fafc;
  transition: all 0.3s ease;
}

.file-upload-box:hover {
  border-color: #3b82f6;
  background: #eff6ff;
}

.file-upload-box.has-file {
  border-color: #3b82f6;
  background: #eff6ff;
  border-style: solid;
}

.cursor-pointer {
  cursor: pointer;
}

.bg-success-soft { background: rgba(16, 185, 129, 0.1); }
.bg-warning-soft { background: rgba(245, 158, 11, 0.1); }
.bg-danger-soft { background: rgba(239, 68, 68, 0.1); }

.text-warning-dark { color: #92400e; }

.extra-small {
  font-size: 0.7rem;
}

.cursor-pointer label {
  cursor: pointer;
}
</style>

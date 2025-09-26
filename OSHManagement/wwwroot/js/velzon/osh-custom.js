/**
 * KTDA OSH Management System - Custom JavaScript
 * Extends Velzon theme functionality for OSH-specific features
 */

// Initialize OSH Management System
document.addEventListener('DOMContentLoaded', function() {
    initializeOSHFeatures();
});

function initializeOSHFeatures() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Initialize compliance meters
    initializeComplianceMeters();
    
    // Initialize risk matrix
    initializeRiskMatrix();
    
    // Initialize form validation
    initializeFormValidation();
    
    // Initialize file upload handlers
    initializeFileUploads();
    
    // Initialize action item handlers
    initializeActionItems();
}

// Compliance Meter Animation
function initializeComplianceMeters() {
    const meters = document.querySelectorAll('.compliance-meter');
    
    meters.forEach(meter => {
        const fill = meter.querySelector('.compliance-meter-fill');
        const percentage = fill.dataset.percentage || 0;
        
        // Animate the meter fill
        setTimeout(() => {
            fill.style.width = percentage + '%';
        }, 500);
        
        // Add color class based on percentage
        if (percentage >= 90) {
            fill.classList.add('compliance-excellent');
        } else if (percentage >= 75) {
            fill.classList.add('compliance-good');
        } else if (percentage >= 50) {
            fill.classList.add('compliance-warning');
        } else {
            fill.classList.add('compliance-critical');
        }
    });
}

// Risk Matrix Functionality
function initializeRiskMatrix() {
    const riskInputs = document.querySelectorAll('.risk-input');
    
    riskInputs.forEach(input => {
        input.addEventListener('change', calculateRiskRating);
    });
}

function calculateRiskRating() {
    const severity = parseInt(document.getElementById('severity')?.value || 1);
    const likelihood = parseInt(document.getElementById('likelihood')?.value || 1);
    const riskRating = severity * likelihood;
    
    const riskRatingElement = document.getElementById('risk-rating');
    if (riskRatingElement) {
        riskRatingElement.textContent = riskRating;
        
        // Update priority based on risk rating
        const priorityElement = document.getElementById('priority');
        if (priorityElement) {
            let priority = 'Low';
            let priorityClass = 'badge bg-success';
            
            if (riskRating >= 6) {
                priority = 'High';
                priorityClass = 'badge bg-danger';
            } else if (riskRating >= 3) {
                priority = 'Medium';
                priorityClass = 'badge bg-warning';
            }
            
            priorityElement.textContent = priority;
            priorityElement.className = priorityClass;
        }
    }
}

// Enhanced Form Validation
function initializeFormValidation() {
    const forms = document.querySelectorAll('.needs-validation');
    
    forms.forEach(form => {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                
                // Focus on first invalid field
                const firstInvalid = form.querySelector(':invalid');
                if (firstInvalid) {
                    firstInvalid.focus();
                    firstInvalid.scrollIntoView({ behavior: 'smooth', block: 'center' });
                }
            }
            
            form.classList.add('was-validated');
        });
    });
    
    // Real-time validation for required fields
    const requiredFields = document.querySelectorAll('input[required], select[required], textarea[required]');
    
    requiredFields.forEach(field => {
        field.addEventListener('blur', function() {
            validateField(field);
        });
        
        field.addEventListener('input', function() {
            if (field.classList.contains('is-invalid')) {
                validateField(field);
            }
        });
    });
}

function validateField(field) {
    const isValid = field.checkValidity();
    
    field.classList.remove('is-valid', 'is-invalid');
    field.classList.add(isValid ? 'is-valid' : 'is-invalid');
    
    return isValid;
}

// File Upload Handlers
function initializeFileUploads() {
    const fileInputs = document.querySelectorAll('input[type="file"]');
    
    fileInputs.forEach(input => {
        input.addEventListener('change', function(e) {
            const files = e.target.files;
            const preview = document.getElementById(input.id + '-preview');
            
            if (preview && files.length > 0) {
                updateFilePreview(files, preview);
            }
        });
    });
}

function updateFilePreview(files, previewContainer) {
    previewContainer.innerHTML = '';
    
    Array.from(files).forEach(file => {
        const fileItem = document.createElement('div');
        fileItem.className = 'file-preview-item d-flex align-items-center mb-2 p-2 border rounded';
        
        const icon = getFileIcon(file.type);
        const size = formatFileSize(file.size);
        
        fileItem.innerHTML = `
            <i class="${icon} me-2"></i>
            <div class="flex-grow-1">
                <div class="fw-medium">${file.name}</div>
                <small class="text-muted">${size}</small>
            </div>
            <button type="button" class="btn btn-sm btn-outline-danger" onclick="removeFile(this)">
                <i class="ri-close-line"></i>
            </button>
        `;
        
        previewContainer.appendChild(fileItem);
    });
}

function getFileIcon(mimeType) {
    if (mimeType.startsWith('image/')) return 'ri-image-line text-success';
    if (mimeType.includes('pdf')) return 'ri-file-pdf-line text-danger';
    if (mimeType.includes('word')) return 'ri-file-word-line text-primary';
    if (mimeType.includes('excel') || mimeType.includes('spreadsheet')) return 'ri-file-excel-line text-success';
    return 'ri-file-line text-secondary';
}

function formatFileSize(bytes) {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
}

function removeFile(button) {
    button.closest('.file-preview-item').remove();
}

// Action Item Management
function initializeActionItems() {
    const actionItems = document.querySelectorAll('.action-item');
    
    actionItems.forEach(item => {
        const statusSelect = item.querySelector('.action-status-select');
        if (statusSelect) {
            statusSelect.addEventListener('change', function() {
                updateActionItemStatus(item, this.value);
            });
        }
    });
}

function updateActionItemStatus(actionItem, status) {
    // Remove existing status classes
    actionItem.classList.remove('action-pending', 'action-in-progress', 'action-completed', 'action-overdue');
    
    // Add new status class
    switch(status.toLowerCase()) {
        case 'pending':
            actionItem.classList.add('action-pending');
            break;
        case 'in-progress':
        case 'inprogress':
            actionItem.classList.add('action-in-progress');
            break;
        case 'completed':
            actionItem.classList.add('action-completed');
            break;
        case 'overdue':
            actionItem.classList.add('action-overdue');
            break;
    }
    
    // Show success message
    showToast('Action item status updated successfully', 'success');
}

// Utility Functions
function showToast(message, type = 'info') {
    const toastContainer = document.getElementById('toast-container') || createToastContainer();
    
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0`;
    toast.setAttribute('role', 'alert');
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    
    toastContainer.appendChild(toast);
    
    const bsToast = new bootstrap.Toast(toast);
    bsToast.show();
    
    // Remove toast element after it's hidden
    toast.addEventListener('hidden.bs.toast', function() {
        toast.remove();
    });
}

function createToastContainer() {
    const container = document.createElement('div');
    container.id = 'toast-container';
    container.className = 'toast-container position-fixed top-0 end-0 p-3';
    container.style.zIndex = '1055';
    document.body.appendChild(container);
    return container;
}

// Confirmation dialogs for critical actions
function confirmAction(message, callback) {
    if (typeof Swal !== 'undefined') {
        Swal.fire({
            title: 'Are you sure?',
            text: message,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#dc3545',
            cancelButtonColor: '#6c757d',
            confirmButtonText: 'Yes, proceed',
            cancelButtonText: 'Cancel'
        }).then((result) => {
            if (result.isConfirmed && callback) {
                callback();
            }
        });
    } else {
        if (confirm(message) && callback) {
            callback();
        }
    }
}

// Export functions for global use
window.OSH = {
    calculateRiskRating,
    updateActionItemStatus,
    showToast,
    confirmAction,
    validateField
};

/**
 * OSH Management System - Form Wizard Component
 * Generic, reusable wizard initialization
 * Works with any FormWizardViewModel automatically
 */

(function ($) {
    'use strict';

    /**
     * Initialize a wizard instance
     * @param {string} wizardId - The ID of the wizard element
     * @param {object} options - Configuration options
     */
    function initializeWizard(wizardId, options) {
        var wizard = $('#' + wizardId);

        if (wizard.length === 0) {
            console.warn('Wizard not found:', wizardId);
            return;
        }

        var settings = $.extend({
            validateOnStepChange: true,
            showProgressBar: false,
            totalSteps: wizard.find('.wizard-step').length,
            onStepChange: null,
            onComplete: null
        }, options);

        var currentStep = 1;
        var totalSteps = settings.totalSteps;

        // ============================================
        // Core Navigation Functions
        // ============================================

        /**
         * Show a specific step
         */
        function showStep(stepNumber) {
            if (stepNumber < 1 || stepNumber > totalSteps) return;

            var isVertical = wizard.hasClass('wizard-vertical');

            // Hide all steps
            wizard.find('.wizard-step').removeClass('active').hide();

            // Handle both horizontal and vertical nav links
            if (isVertical) {
                wizard.find('.wizard-nav-link-vertical').removeClass('active');
                wizard.find('.wizard-step-item').removeClass('active');

                // Mark completed steps
                wizard.find('.wizard-step-item').each(function() {
                    var itemStep = parseInt($(this).data('step'));
                    if (itemStep < stepNumber) {
                        $(this).addClass('completed');
                    } else {
                        $(this).removeClass('completed');
                    }
                });
            } else {
                wizard.find('.wizard-nav-link').removeClass('active');
            }

            // Show target step
            wizard.find('.wizard-step[data-step="' + stepNumber + '"]').addClass('active').fadeIn(300);

            if (isVertical) {
                wizard.find('.wizard-nav-link-vertical[data-step="' + stepNumber + '"]').addClass('active');
                wizard.find('.wizard-step-item[data-step="' + stepNumber + '"]').addClass('active');
            } else {
                wizard.find('.wizard-nav-link[data-step="' + stepNumber + '"]').addClass('active');
            }

            // Update UI
            updateButtons(stepNumber);
            updateProgress(stepNumber);
            currentStep = stepNumber;

            // Scroll to wizard (different behavior for vertical)
            if (!isVertical) {
                wizard.get(0).scrollIntoView({ behavior: 'smooth', block: 'start' });
            }

            // Callback
            if (typeof settings.onStepChange === 'function') {
                settings.onStepChange(stepNumber, totalSteps);
            }
        }

        /**
         * Update navigation buttons visibility
         */
        function updateButtons(stepNumber) {
            var prevBtn = wizard.find('.wizard-prev');
            var nextBtn = wizard.find('.wizard-next');
            var submitBtn = wizard.find('.wizard-submit');

            prevBtn.toggle(stepNumber > 1);
            nextBtn.toggle(stepNumber < totalSteps);
            submitBtn.toggle(stepNumber === totalSteps);
        }

        /**
         * Update progress bar and step counters
         */
        function updateProgress(stepNumber) {
            var isVertical = wizard.hasClass('wizard-vertical');

            // Update horizontal progress bar
            if (settings.showProgressBar && !isVertical) {
                var percentage = Math.round((stepNumber / totalSteps) * 100);
                var progressBar = wizard.find('.wizard-progress-bar');
                var progressText = wizard.find('.wizard-progress-text');

                if (progressBar.length > 0) {
                    progressBar.css('width', percentage + '%').attr('aria-valuenow', percentage);
                }

                if (progressText.length > 0) {
                    progressText.text('Step ' + stepNumber + ' of ' + totalSteps);
                }

                // Update percentage display if exists
                var progressPercentage = wizard.find('.wizard-progress-percentage');
                if (progressPercentage.length > 0) {
                    progressPercentage.text(percentage + '%');
                }
            }

            // Update vertical progress summary
            if (isVertical) {
                // Update current step number
                wizard.find('.wizard-current-step').text(stepNumber);

                // Count completed steps
                var completedCount = wizard.find('.wizard-step-item.completed').length;
                wizard.find('.wizard-completed-count').text(completedCount);
            }
        }

        // ============================================
        // Validation Functions
        // ============================================

        /**
         * Validate a specific step
         */
        function validateStep(stepNumber) {
            if (!settings.validateOnStepChange) return true;

            var stepElement = wizard.find('.wizard-step[data-step="' + stepNumber + '"]');
            var isValid = true;
            var firstInvalidField = null;

            // Clear previous validation
            stepElement.find('.is-invalid').removeClass('is-invalid');
            stepElement.find('.invalid-feedback').hide();

            // If jQuery Validation is available, use it for validation including Remote validation
            var form = wizard.closest('form');
            if (form.length > 0 && typeof $.fn.validate !== 'undefined') {
                var validator = form.validate();

                // Validate all fields in current step
                stepElement.find('input, select, textarea').each(function () {
                    var field = $(this);

                    // Trigger validation for this field
                    if (!validator.element(field)) {
                        // Field is invalid
                        field.addClass('is-invalid');

                        // Show error message
                        var errorMessage = field.data('val-remote') ||
                                         field.data('val-required') ||
                                         field.data('val-email') ||
                                         'Invalid value';

                        var errorSpan = field.next('.field-validation-error, .invalid-feedback');
                        if (errorSpan.length > 0) {
                            errorSpan.show();
                        }

                        if (!firstInvalidField) firstInvalidField = field;
                        isValid = false;
                    } else {
                        // Field is valid
                        field.removeClass('is-invalid');
                    }
                });

                // If validation failed, focus first invalid field
                if (!isValid && firstInvalidField) {
                    firstInvalidField.focus();
                }

                return isValid;
            }

            // Fallback: Manual validation if jQuery Validation not available

            // Validate required fields
            stepElement.find('input[required], select[required], textarea[required]').each(function () {
                var field = $(this);
                var value = field.val();

                if (!value || (typeof value === 'string' && value.trim() === '')) {
                    markFieldInvalid(field, field.data('val-required') || 'This field is required');
                    if (!firstInvalidField) firstInvalidField = field;
                    isValid = false;
                }
            });

            // Validate email fields
            stepElement.find('input[type="email"]').each(function () {
                var field = $(this);
                var value = field.val();

                if (value && !isValidEmail(value)) {
                    markFieldInvalid(field, 'Please enter a valid email address');
                    if (!firstInvalidField) firstInvalidField = field;
                    isValid = false;
                }
            });

            // Validate number fields
            stepElement.find('input[type="number"]').each(function () {
                var field = $(this);
                var value = field.val();
                var min = field.attr('min');
                var max = field.attr('max');

                if (value) {
                    var numValue = parseFloat(value);
                    if (min && numValue < parseFloat(min)) {
                        markFieldInvalid(field, 'Value must be at least ' + min);
                        if (!firstInvalidField) firstInvalidField = field;
                        isValid = false;
                    }
                    if (max && numValue > parseFloat(max)) {
                        markFieldInvalid(field, 'Value must be at most ' + max);
                        if (!firstInvalidField) firstInvalidField = field;
                        isValid = false;
                    }
                }
            });

            // Focus first invalid field
            if (!isValid && firstInvalidField) {
                firstInvalidField.focus();
            }

            return isValid;
        }

        /**
         * Mark a field as invalid
         */
        function markFieldInvalid(field, message) {
            field.addClass('is-invalid');
            field.siblings('.invalid-feedback').show().text(message);
        }

        /**
         * Email validation helper
         */
        function isValidEmail(email) {
            return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
        }

        /**
         * Validate all steps before submission
         */
        function validateAllSteps() {
            for (var i = 1; i <= totalSteps; i++) {
                if (!validateStep(i)) {
                    showStep(i); // Show first invalid step
                    return false;
                }
            }
            return true;
        }

        // ============================================
        // Event Handlers
        // ============================================

        // Next button
        wizard.find('.wizard-next').on('click', function (e) {
            e.preventDefault();
            if (validateStep(currentStep)) {
                showStep(currentStep + 1);
            }
        });

        // Previous button
        wizard.find('.wizard-prev').on('click', function (e) {
            e.preventDefault();
            showStep(currentStep - 1);
        });

        // Tab navigation (handles both horizontal and vertical)
        wizard.find('.wizard-nav-link, .wizard-nav-link-vertical').on('click', function (e) {
            e.preventDefault();
            var targetStep = parseInt($(this).data('step'));

            // Allow going back without validation
            if (targetStep < currentStep) {
                showStep(targetStep);
            }
            // Validate before jumping forward
            else if (targetStep > currentStep) {
                var canProceed = true;
                for (var i = currentStep; i < targetStep; i++) {
                    if (!validateStep(i)) {
                        canProceed = false;
                        break;
                    }
                }
                if (canProceed) {
                    showStep(targetStep);
                }
            }
        });

        // Submit button
        wizard.find('.wizard-submit').on('click', function (e) {
            e.preventDefault();

            if (validateAllSteps()) {
                // Callback before submission
                if (typeof settings.onComplete === 'function') {
                    if (settings.onComplete(wizard) === false) {
                        return; // Allow callback to cancel submission
                    }
                }

                // Submit the form
                wizard.submit();
            }
        });

        // Enter key navigation
        wizard.find('input, select, textarea').on('keypress', function (e) {
            if (e.which === 13 && !$(this).is('textarea')) {
                e.preventDefault();
                if (currentStep < totalSteps) {
                    wizard.find('.wizard-next').trigger('click');
                } else {
                    wizard.find('.wizard-submit').trigger('click');
                }
            }
        });

        // ============================================
        // Initialize
        // ============================================
        console.log('Wizard initialized:', {
            wizardId: wizardId,
            totalSteps: totalSteps,
            validateOnStepChange: settings.validateOnStepChange,
            showProgressBar: settings.showProgressBar
        });

        showStep(1);

        // Return public API
        return {
            showStep: showStep,
            validateStep: validateStep,
            validateAllSteps: validateAllSteps,
            getCurrentStep: function () { return currentStep; },
            getTotalSteps: function () { return totalSteps; }
        };
    }

    // ============================================
    // Auto-initialization
    // ============================================

    /**
     * Automatically initialize all wizards on the page
     */
    function autoInitWizards() {
        $('.wizard-form').each(function () {
            var $form = $(this);
            var wizardId = $form.attr('id');

            if (wizardId && !$form.data('wizard-initialized')) {
                var validateOnStepChange = $form.data('validate-on-step-change') !== false;
                var showProgressBar = $form.data('show-progress-bar') === true;
                var totalSteps = $form.data('total-steps') || $form.find('.wizard-step').length;

                initializeWizard(wizardId, {
                    validateOnStepChange: validateOnStepChange,
                    showProgressBar: showProgressBar,
                    totalSteps: totalSteps
                });

                $form.data('wizard-initialized', true);
            }
        });
    }

    // ============================================
    // Public API
    // ============================================

    // Expose to global scope
    window.FormWizard = {
        initialize: initializeWizard,
        autoInit: autoInitWizards
    };

    // Auto-initialize on document ready
    $(document).ready(function () {
        autoInitWizards();
    });

})(jQuery);

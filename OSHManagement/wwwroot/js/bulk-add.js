// BulkAdd Component JavaScript
// Follows the same pattern as other components (DataTable, Form, etc.)
(function() {
    'use strict';
    
    class BulkAddComponent {
        constructor(config) {
            this.config = config;
            this.componentId = config.componentId;
            this.rowCount = 0;
            this.rowsContainer = document.getElementById(`${this.componentId}_rowsContainer`);
            this.addButton = document.getElementById(`${this.componentId}_addRow`);
            this.template = document.getElementById(`${this.componentId}_rowTemplate`);
            this.parentSelector = config.parentSelector ? 
                document.getElementById(`parentSelector_${config.parentParameterName}`) : null;
            
            this.init();
        }
        
        init() {
            // Add initial rows
            for (let i = 0; i < this.config.initialRows; i++) {
                this.addRow();
            }
            
            // Attach event listeners
            this.addButton.addEventListener('click', () => this.addRow());
            
            // Handle parent selector filtering
            if (this.parentSelector && this.config.filtersRowFields) {
                this.parentSelector.addEventListener('change', () => this.filterAllRows());
            }
            
            // Handle cascading parent selectors (e.g., Category -> Station)
            this.setupCascadingSelectors();
            
            // Update submit button text
            this.updateSubmitText();
        }
        
        addRow() {
            if (this.rowCount >= this.config.maxRows) {
                alert(`Maximum ${this.config.maxRows} ${this.config.entityNamePlural} allowed`);
                return;
            }
            
            // Clone template
            const templateContent = this.template.content.cloneNode(true);
            const rowDiv = templateContent.querySelector('.bulk-add-row');
            
            // Replace placeholders
            const index = this.rowCount;
            const number = this.rowCount + 1;
            
            rowDiv.innerHTML = rowDiv.innerHTML
                .replace(/\{\{index\}\}/g, index)
                .replace(/\{\{number\}\}/g, number);
            
            rowDiv.setAttribute('data-row-index', index);
            
            // Append to container
            this.rowsContainer.appendChild(rowDiv);
            
            // Attach remove handler
            const removeBtn = rowDiv.querySelector('.remove-row');
            removeBtn.addEventListener('click', () => this.removeRow(rowDiv));
            
            this.rowCount++;
            this.updateRowNumbers();
            this.updateSubmitText();
            
            // Filter if parent is selected
            if (this.parentSelector && this.config.filtersRowFields) {
                this.filterRow(rowDiv);
            }
        }
        
        removeRow(rowDiv) {
            const totalRows = this.rowsContainer.querySelectorAll('.bulk-add-row').length;
            
            if (totalRows <= this.config.minRows) {
                alert(`Minimum ${this.config.minRows} ${this.config.entityName}(s) required`);
                return;
            }
            
            rowDiv.remove();
            this.updateRowNumbers();
            this.updateSubmitText();
        }
        
        updateRowNumbers() {
            const rows = this.rowsContainer.querySelectorAll('.bulk-add-row');
            rows.forEach((row, index) => {
                const badge = row.querySelector('.row-number');
                if (badge) {
                    badge.textContent = index + 1;
                }
                
                // Update field names with correct index
                const fields = row.querySelectorAll('.bulk-add-field');
                fields.forEach(field => {
                    const name = field.getAttribute('name');
                    if (name) {
                        field.setAttribute('name', name.replace(/Items\[\d+\]/, `Items[${index}]`));
                    }
                    
                    const id = field.getAttribute('id');
                    if (id) {
                        field.setAttribute('id', id.replace(/_\d+_/, `_${index}_`));
                    }
                });
            });
            
            // Update count display
            const countSpan = document.getElementById(`${this.componentId}_currentCount`);
            if (countSpan) {
                countSpan.textContent = rows.length;
            }
        }
        
        updateSubmitText() {
            const count = this.rowsContainer.querySelectorAll('.bulk-add-row').length;
            const submitText = document.getElementById(`${this.componentId}_submitText`);
            if (submitText) {
                submitText.textContent = `Save ${count} ${count === 1 ? this.config.entityName : this.config.entityNamePlural}`;
            }
        }
        
        filterAllRows() {
            const rows = this.rowsContainer.querySelectorAll('.bulk-add-row');
            rows.forEach(row => this.filterRow(row));
        }
        
        filterRow(row) {
            if (!this.parentSelector) return;
            
            const parentValue = this.parentSelector.value;
            const filterFields = row.querySelectorAll('.filter-by-parent');
            
            filterFields.forEach(field => {
                const filterProperty = field.getAttribute('data-filter-property');
                const options = field.querySelectorAll('option:not(:first-child)');
                
                if (!parentValue) {
                    // Show all options
                    options.forEach(opt => opt.style.display = '');
                } else {
                    // Filter options based on parent value
                    options.forEach(opt => {
                        const filterValue = opt.getAttribute('data-filter-value');
                        // For now, simple equality check
                        // In production, you might need more complex filtering logic
                        opt.style.display = filterValue === parentValue ? '' : 'none';
                    });
                }
                
                // Reset selection if current value is hidden
                const currentOption = field.querySelector(`option[value="${field.value}"]`);
                if (currentOption && currentOption.style.display === 'none') {
                    field.value = '';
                }
            });
        }
        
        setupCascadingSelectors() {
            // Find all parent selectors that filter other selectors
            const allSelectors = document.querySelectorAll(`#${this.componentId} select[data-filters-other="true"]`);
            
            allSelectors.forEach(selector => {
                const targetParam = selector.getAttribute('data-filter-target');
                if (!targetParam) return;
                
                const targetSelector = document.getElementById(`parentSelector_${targetParam}`);
                if (!targetSelector) return;
                
                // Store all options from target
                const allOptions = Array.from(targetSelector.querySelectorAll('option:not(:first-child)'));
                const optionsData = allOptions.map(opt => ({
                    value: opt.value,
                    text: opt.textContent,
                    filterValue: opt.getAttribute('data-category-id') || opt.value
                }));
                
                selector.addEventListener('change', () => {
                    const selectedValue = selector.value;
                    const currentSelection = targetSelector.value;
                    
                    // Remove all options except first
                    targetSelector.querySelectorAll('option:not(:first-child)').forEach(opt => opt.remove());
                    
                    if (!selectedValue) {
                        // Show all options
                        optionsData.forEach(data => {
                            const option = document.createElement('option');
                            option.value = data.value;
                            option.textContent = data.text;
                            if (data.value === currentSelection) {
                                option.selected = true;
                            }
                            targetSelector.appendChild(option);
                        });
                    } else {
                        // Filter options - need to get station data from window
                        const stationsData = window[`${this.componentId}_stationsData`] || [];
                        const filteredStations = stationsData.filter(s => s.orgCategoryId == selectedValue);
                        
                        filteredStations.forEach(station => {
                            const option = document.createElement('option');
                            option.value = station.stationId;
                            option.textContent = station.stationName;
                            if (station.stationId == currentSelection) {
                                option.selected = true;
                            }
                            targetSelector.appendChild(option);
                        });
                        
                        // Clear selection if not in filtered list
                        if (!filteredStations.find(s => s.stationId == currentSelection)) {
                            targetSelector.value = '';
                        }
                    }
                });
            });
        }
    }
    
    // Auto-initialize all BulkAdd components on page
    document.addEventListener('DOMContentLoaded', function() {
        // Find all bulk add configs in window object
        Object.keys(window).forEach(key => {
            if (key.endsWith('_config') && window[key].componentId) {
                new BulkAddComponent(window[key]);
            }
        });
    });
})();

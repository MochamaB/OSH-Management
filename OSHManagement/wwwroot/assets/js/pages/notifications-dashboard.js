// Notifications Dashboard Charts and Interactions

(function() {
    'use strict';

    // Get data from hidden elements or page
    const stats = {
        infoCount: parseInt($('#info-count').val() || 0),
        successCount: parseInt($('#success-count').val() || 0),
        warningCount: parseInt($('#warning-count').val() || 0),
        errorCount: parseInt($('#error-count').val() || 0),
        actionCount: parseInt($('#action-count').val() || 0),
        urgentCount: parseInt($('#urgent-count').val() || 0),
        highCount: parseInt($('#high-count').val() || 0),
        normalCount: parseInt($('#normal-count').val() || 0),
        lowCount: parseInt($('#low-count').val() || 0),
        totalCount: parseInt($('#total-count').val() || 0)
    };

    // Initialize charts
    initTypeChart();
    initPriorityChart();
    loadRecentNotifications();
    initEventHandlers();

    // Notification Type Bar Chart
    function initTypeChart() {
        // Get values from the model via data attributes or calculate from page
        const chartElement = document.querySelector("#notification-type-chart");
        if (!chartElement) return;

        // Extract data from the card badges or use passed values
        const infoCount = parseInt(chartElement.dataset.info || stats.infoCount);
        const successCount = parseInt(chartElement.dataset.success || stats.successCount);
        const warningCount = parseInt(chartElement.dataset.warning || stats.warningCount);
        const errorCount = parseInt(chartElement.dataset.error || stats.errorCount);
        const actionCount = parseInt(chartElement.dataset.action || stats.actionCount);

        const typeChartOptions = {
            series: [{
                name: 'Notifications',
                data: [infoCount, successCount, warningCount, errorCount, actionCount]
            }],
            chart: {
                type: 'bar',
                height: 300,
                toolbar: {
                    show: false
                }
            },
            plotOptions: {
                bar: {
                    horizontal: true,
                    borderRadius: 4,
                    distributed: true,
                    dataLabels: {
                        position: 'top'
                    }
                }
            },
            colors: ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6'],
            dataLabels: {
                enabled: true,
                style: {
                    colors: ['#fff'],
                    fontSize: '12px'
                }
            },
            xaxis: {
                categories: ['Info', 'Success', 'Warning', 'Error', 'Action Required'],
                labels: {
                    style: {
                        fontSize: '12px'
                    }
                }
            },
            yaxis: {
                labels: {
                    style: {
                        fontSize: '12px'
                    }
                }
            },
            legend: {
                show: false
            },
            grid: {
                borderColor: '#f1f1f1',
                strokeDashArray: 3
            },
            tooltip: {
                theme: 'dark',
                y: {
                    formatter: function(val) {
                        return val + " notifications"
                    }
                }
            }
        };

        const typeChart = new ApexCharts(chartElement, typeChartOptions);
        typeChart.render();
    }

    // Priority Donut Chart
    function initPriorityChart() {
        const chartElement = document.querySelector("#priority-donut-chart");
        if (!chartElement) return;

        const urgentCount = parseInt(chartElement.dataset.urgent || stats.urgentCount);
        const highCount = parseInt(chartElement.dataset.high || stats.highCount);
        const normalCount = parseInt(chartElement.dataset.normal || stats.normalCount);
        const lowCount = parseInt(chartElement.dataset.low || stats.lowCount);
        const totalCount = parseInt(chartElement.dataset.total || stats.totalCount);

        const priorityChartOptions = {
            series: [urgentCount, highCount, normalCount, lowCount],
            chart: {
                type: 'donut',
                height: 250
            },
            labels: ['Urgent', 'High', 'Normal', 'Low'],
            colors: ['#ef4444', '#f59e0b', '#6366f1', '#6b7280'],
            legend: {
                show: false
            },
            dataLabels: {
                enabled: true,
                formatter: function(val) {
                    return Math.round(val) + "%"
                },
                style: {
                    fontSize: '12px'
                }
            },
            plotOptions: {
                pie: {
                    donut: {
                        size: '70%',
                        labels: {
                            show: true,
                            name: {
                                show: true,
                                fontSize: '14px'
                            },
                            value: {
                                show: true,
                                fontSize: '20px',
                                fontWeight: 600
                            },
                            total: {
                                show: true,
                                label: 'Total',
                                fontSize: '14px',
                                formatter: function(w) {
                                    return totalCount || 0;
                                }
                            }
                        }
                    }
                }
            },
            tooltip: {
                theme: 'dark',
                y: {
                    formatter: function(val) {
                        return val + " notifications"
                    }
                }
            }
        };

        const priorityChart = new ApexCharts(chartElement, priorityChartOptions);
        priorityChart.render();
    }

    // Load Recent Notifications via AJAX
    function loadRecentNotifications() {
        const tbody = $('#recent-notifications-tbody');
        
        $.ajax({
            url: '/Notifications/GetRecent',
            type: 'GET',
            data: { count: 10 },
            success: function(response) {
                if (response.success && response.data && response.data.length > 0) {
                    tbody.empty();
                    
                    response.data.forEach(function(notification) {
                        const typeIcon = getTypeIcon(notification.notificationType);
                        const priorityBadge = getPriorityBadge(notification.priority);
                        const statusBadge = notification.isRead 
                            ? '<span class="badge bg-success-transparent">Read</span>' 
                            : '<span class="badge bg-warning-transparent">Unread</span>';
                        
                        const categoryBadge = notification.category 
                            ? `<span class="badge bg-light text-dark">${notification.category}</span>`
                            : '<span class="text-muted">N/A</span>';

                        const row = `
                            <tr class="${!notification.isRead ? 'table-active' : ''}">
                                <td>${typeIcon}</td>
                                <td>
                                    <div class="fw-semibold">${escapeHtml(notification.title)}</div>
                                    <small class="text-muted">${truncate(escapeHtml(notification.message), 50)}</small>
                                </td>
                                <td>${categoryBadge}</td>
                                <td>${priorityBadge}</td>
                                <td class="text-muted">${notification.timeAgo}</td>
                                <td>${statusBadge}</td>
                            </tr>
                        `;
                        tbody.append(row);
                    });
                } else {
                    tbody.html(`
                        <tr>
                            <td colspan="6" class="text-center py-4 text-muted">
                                <i class="ti ti-bell-off fs-40 mb-2 d-block"></i>
                                <p class="mb-0">No recent notifications</p>
                            </td>
                        </tr>
                    `);
                }
            },
            error: function(xhr, status, error) {
                console.error('Error loading notifications:', error);
                tbody.html(`
                    <tr>
                        <td colspan="6" class="text-center py-4 text-danger">
                            <i class="ti ti-alert-circle fs-40 mb-2 d-block"></i>
                            <p class="mb-0">Error loading notifications</p>
                        </td>
                    </tr>
                `);
            }
        });
    }

    // Initialize Event Handlers
    function initEventHandlers() {
        // Mark all as read
        $('#mark-all-read-btn').on('click', function() {
            if (confirm('Mark all notifications as read?')) {
                const btn = $(this);
                btn.prop('disabled', true).html('<span class="spinner-border spinner-border-sm me-1"></span>Processing...');
                
                $.ajax({
                    url: '/Notifications/MarkAllAsRead',
                    type: 'POST',
                    success: function(response) {
                        if (response.success) {
                            if (typeof toastr !== 'undefined') {
                                toastr.success(response.message || 'All notifications marked as read');
                            } else {
                                alert(response.message || 'All notifications marked as read');
                            }
                            setTimeout(function() {
                                location.reload();
                            }, 1000);
                        } else {
                            if (typeof toastr !== 'undefined') {
                                toastr.error(response.message || 'Error marking notifications as read');
                            } else {
                                alert(response.message || 'Error marking notifications as read');
                            }
                            btn.prop('disabled', false).html('<i class="ti ti-check me-1"></i>Mark All as Read');
                        }
                    },
                    error: function(xhr, status, error) {
                        console.error('Error:', error);
                        if (typeof toastr !== 'undefined') {
                            toastr.error('Error marking notifications as read');
                        } else {
                            alert('Error marking notifications as read');
                        }
                        btn.prop('disabled', false).html('<i class="ti ti-check me-1"></i>Mark All as Read');
                    }
                });
            }
        });

        // Auto-refresh every 30 seconds
        setInterval(function() {
            loadRecentNotifications();
        }, 30000);
    }

    // Helper Functions
    function getTypeIcon(type) {
        const icons = {
            'Info': '<i class="ti ti-info-circle text-primary fs-18"></i>',
            'Success': '<i class="ti ti-check-circle text-success fs-18"></i>',
            'Warning': '<i class="ti ti-alert-triangle text-warning fs-18"></i>',
            'Error': '<i class="ti ti-x-circle text-danger fs-18"></i>',
            'ActionRequired': '<i class="ti ti-urgent text-purple fs-18"></i>'
        };
        return icons[type] || '<i class="ti ti-bell text-muted fs-18"></i>';
    }

    function getPriorityBadge(priority) {
        const badges = {
            'Urgent': '<span class="badge bg-danger">Urgent</span>',
            'High': '<span class="badge bg-warning">High</span>',
            'Normal': '<span class="badge bg-primary">Normal</span>',
            'Low': '<span class="badge bg-secondary">Low</span>'
        };
        return badges[priority] || '<span class="badge bg-secondary">N/A</span>';
    }

    function truncate(str, maxLength) {
        if (!str) return '';
        return str.length > maxLength ? str.substring(0, maxLength) + '...' : str;
    }

    function escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, function(m) { return map[m]; });
    }

})();

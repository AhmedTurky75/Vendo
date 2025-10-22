import { Component } from '@angular/core';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent {
  stats = [
    { title: 'Total Tenants', value: '24', change: '+12%', icon: '🏢' },
    { title: 'Active Merchants', value: '156', change: '+8%', icon: '🛍️' },
    { title: 'Total Revenue', value: '$45,231', change: '+23%', icon: '💰' },
    { title: 'Active Customers', value: '2,345', change: '+15%', icon: '👥' }
  ];

  recentActivities = [
    { action: 'New tenant registered', tenant: 'Acme Corp', time: '2 hours ago' },
    { action: 'Merchant store created', tenant: 'TechStore', time: '5 hours ago' },
    { action: 'Payment processed', tenant: 'Fashion Hub', time: '1 day ago' }
  ];
}

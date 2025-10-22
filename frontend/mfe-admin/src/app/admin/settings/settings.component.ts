import { Component } from '@angular/core';

@Component({
  selector: 'app-settings',
  templateUrl: './settings.component.html',
  styleUrls: ['./settings.component.css']
})
export class SettingsComponent {
  settings = {
    platformName: 'Vendo Multi-Tenant Platform',
    adminEmail: 'admin@vendo.com',
    maxTenantsPerPlan: 100,
    enableRegistrations: true,
    maintenanceMode: false
  };
}

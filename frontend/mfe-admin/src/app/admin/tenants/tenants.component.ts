import { Component } from '@angular/core';

@Component({
  selector: 'app-tenants',
    standalone: false,
  templateUrl: './tenants.component.html',
  styleUrls: ['./tenants.component.css']
})
export class TenantsComponent {
  tenants = [
    { id: 1, name: 'Acme Corporation', domain: 'acme.vendo.com', status: 'Active', merchants: 15, created: '2024-01-15' },
    { id: 2, name: 'TechStore Inc', domain: 'techstore.vendo.com', status: 'Active', merchants: 8, created: '2024-02-20' },
    { id: 3, name: 'Fashion Hub', domain: 'fashionhub.vendo.com', status: 'Active', merchants: 23, created: '2024-03-10' },
    { id: 4, name: 'Global Retail', domain: 'globalretail.vendo.com', status: 'Inactive', merchants: 0, created: '2024-04-05' }
  ];
}

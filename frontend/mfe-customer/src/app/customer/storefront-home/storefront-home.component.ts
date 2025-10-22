import { Component, OnInit } from '@angular/core';

interface Product {
  id: string;
  name: string;
  price: number;
  image: string;
  category: string;
  rating: number;
}

@Component({
  selector: 'app-storefront-home',
  standalone: false,
  templateUrl: './storefront-home.component.html',
  styleUrls: ['./storefront-home.component.css']
})
export class StorefrontHomeComponent implements OnInit {
  products: Product[] = [
    { id: '1', name: 'Premium Headphones', price: 199.99, image: '🎧', category: 'Electronics', rating: 4.5 },
    { id: '2', name: 'Wireless Mouse', price: 49.99, image: '🖱️', category: 'Electronics', rating: 4.2 },
    { id: '3', name: 'Laptop Stand', price: 79.99, image: '💻', category: 'Accessories', rating: 4.7 },
    { id: '4', name: 'USB-C Hub', price: 89.99, image: '🔌', category: 'Accessories', rating: 4.4 },
    { id: '5', name: 'Mechanical Keyboard', price: 149.99, image: '⌨️', category: 'Electronics', rating: 4.8 },
    { id: '6', name: 'Desk Lamp', price: 59.99, image: '💡', category: 'Accessories', rating: 4.3 },
    { id: '7', name: 'Monitor', price: 299.99, image: '🖥️', category: 'Electronics', rating: 4.6 },
    { id: '8', name: 'Webcam', price: 129.99, image: '📷', category: 'Electronics', rating: 4.1 }
  ];

  featuredProducts: Product[] = [];
  selectedCategory: string = 'all';
  categories: string[] = ['all', 'Electronics', 'Accessories'];

  ngOnInit(): void {
    this.featuredProducts = this.products.slice(0, 4);
  }

  getFilteredProducts(): Product[] {
    if (this.selectedCategory === 'all') {
      return this.products;
    }
    return this.products.filter(p => p.category === this.selectedCategory);
  }

  selectCategory(category: string): void {
    this.selectedCategory = category;
  }
}

import { Routes } from '@angular/router';

const routes: Routes = [
  { path: '', redirectTo: 'customer/login', pathMatch: 'full' },
  {
    path: 'customer',
    children: [
      {
        path: 'login',
        loadComponent: () =>
          import('./customer/login/login').then(m => m.LoginComponent)
      },
      {
        path: 'tracking/:id',
        loadComponent: () =>
          import('./customer/tracking/tracking').then(m => m.TrackingComponent)
      },
      {
        path: 'delivery-request',
        loadComponent: () =>
          import('./customer/delivery-request/delivery-request').then(m => m.DeliveryRequestComponent)
      },
      {
        path: 'my-deliveries',
        loadComponent: () =>
          import('./customer/my-deliveries/my-deliveries').then(m => m.MyDeliveriesComponent)
      }
    ]
  },
  {
    path: 'staff',
    children: [
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./staff/dashboard/dashboard').then(m => m.DashboardComponent)
      },
      {
        path: 'drone-tracker',
        loadComponent: () =>
          import('./staff/drone-tracker/drone-tracker').then(m => m.DroneTracker)
      },
      {
        path: 'dispatch',
        loadComponent: () =>
          import('./staff/dispatch/dispatch').then(m => m.Dispatch)
      },
      {
        path: 'all-packages',
        loadComponent: () =>
          import('./staff/all-packages/all-packages').then(m => m.AllPackages)
      }
    ]
  }
];

export default routes;
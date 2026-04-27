import React, { useState, useEffect } from 'react';
import { productApi, userApi, categoryApi } from '../services/api';
import { useAuth } from '../contexts/AuthContext';

const Dashboard = () => {
    const [stats, setStats] = useState({
        products: 0,
        categories: 0,
        motorcycles: 0,
        accessories: 0,
        users: 0,
    });
    const [loading, setLoading] = useState(true);
    const { isAdmin } = useAuth();

    useEffect(() => {
        loadStats();
    }, []);

    const loadStats = async () => {
        try {
            const [productsRes, motorcyclesRes, accessoriesRes, categoriesRes] = await Promise.all([
                productApi.getAll(),
                productApi.getAll({ productType: 'Motorcycle', pageSize: 1 }),
                productApi.getAll({ productType: 'Accessory', pageSize: 1 }),
                categoryApi.getAll(),
            ]);

            let usersCount = 0;
            if (isAdmin()) {
                try {
                    const usersRes = await userApi.getAll({ page: 1, pageSize: 1 });
                    usersCount = usersRes.data.totalCount || 0;
                } catch (e) {
                    console.log('Cannot fetch users count');
                }
            }

            setStats({
                products: productsRes.data?.totalCount || productsRes.data?.items?.length || productsRes.data?.length || 0,
                motorcycles: motorcyclesRes.data?.totalCount || 0,
                accessories: accessoriesRes.data?.totalCount || 0,
                categories: categoriesRes.data?.length || 0,
                users: usersCount,
            });
        } catch (error) {
            console.error('Failed to load stats:', error);
        } finally {
            setLoading(false);
        }
    };

    return (
        <div className="content-wrapper">
            <div className="content-header">
                <div className="container-fluid">
                    <div className="row mb-2">
                        <div className="col-sm-6">
                            <h1 className="m-0">Motorcycle Showroom Dashboard</h1>
                        </div>
                    </div>
                </div>
            </div>

            <section className="content">
                <div className="container-fluid">
                    {loading ? (
                        <div className="text-center py-5">
                            <div className="spinner-border text-primary" role="status">
                                <span className="sr-only">Loading...</span>
                            </div>
                        </div>
                    ) : (
                        <div className="row">
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-info">
                                    <div className="inner">
                                        <h3>{stats.products}</h3>
                                        <p>Catalog Items</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-box"></i>
                                    </div>
                                    <a href="/products" className="small-box-footer">
                                        More info <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-success">
                                    <div className="inner">
                                        <h3>{stats.categories}</h3>
                                        <p>Categories</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-tags"></i>
                                    </div>
                                    <a href="/categories" className="small-box-footer">
                                        More info <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>
                            {isAdmin() && (
                                <div className="col-lg-3 col-6">
                                    <div className="small-box bg-warning">
                                        <div className="inner">
                                            <h3>{stats.users}</h3>
                                            <p>Users</p>
                                        </div>
                                        <div className="icon">
                                            <i className="fas fa-users"></i>
                                        </div>
                                        <a href="/users" className="small-box-footer">
                                            More info <i className="fas fa-arrow-circle-right"></i>
                                        </a>
                                    </div>
                                </div>
                            )}
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-primary">
                                    <div className="inner">
                                        <h3>{stats.motorcycles}</h3>
                                        <p>Motorcycles</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-motorcycle"></i>
                                    </div>
                                    <a href="/products" className="small-box-footer">
                                        More info <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>
                            <div className="col-lg-3 col-6">
                                <div className="small-box bg-secondary">
                                    <div className="inner">
                                        <h3>{stats.accessories}</h3>
                                        <p>Accessories</p>
                                    </div>
                                    <div className="icon">
                                        <i className="fas fa-tools"></i>
                                    </div>
                                    <a href="/products" className="small-box-footer">
                                        More info <i className="fas fa-arrow-circle-right"></i>
                                    </a>
                                </div>
                            </div>
                        </div>
                    )}

                    <div className="row">
                        <div className="col-12">
                            <div className="card">
                                <div className="card-header">
                                    <h3 className="card-title">Motorcycle showroom MVP modules</h3>
                                </div>
                                <div className="card-body">
                                    <p>This admin app now focuses on showroom catalog and sales operations:</p>
                                    <ul>
                                        <li><strong>Catalog:</strong> motorcycles, accessories, brands, models, categories and showroom location</li>
                                        <li><strong>Commerce:</strong> stock status, order snapshots, payment status and voucher-ready totals</li>
                                        <li><strong>Content:</strong> contact requests, FAQ and blog entities are available in the backend model</li>
                                        <li><strong>Access:</strong> Customer, Staff and Admin roles are represented by the existing user type flow</li>
                                    </ul>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    );
};

export default Dashboard;

import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  poweredByHeader: false,
  async rewrites() {
    const backend = process.env.API_INTERNAL_BASE_URL ?? "http://127.0.0.1:5000";
    return {
      beforeFiles: [
        { source: "/api/:path*", destination: `${backend}/api/:path*` },
        { source: "/auth/:path*", destination: `${backend}/auth/:path*` },
        { source: "/signin-oidc", destination: `${backend}/signin-oidc` },
        { source: "/signout-callback-oidc", destination: `${backend}/signout-callback-oidc` },
      ],
    };
  },
};

export default nextConfig;

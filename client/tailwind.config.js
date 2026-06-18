/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.js'],
  theme: {
    extend: {
      colors: {
        ink: '#1f2933',
        parchment: '#f8f4ec',
        cedar: '#8a5a44',
        moss: '#3f6f5e',
        river: '#256d85',
      },
      boxShadow: {
        soft: '0 18px 50px rgba(31, 41, 51, 0.12)',
      },
    },
  },
  plugins: [],
};

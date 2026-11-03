// Copies the vendor bundles the reference page needs. antd 6 still ships a UMD build, but
// React 19 no longer does, so the page is pinned to React 18 UMD.
const fs = require('fs');
const path = require('path');
const https = require('https');

const here = __dirname;
const vendor = path.join(here, 'vendor');
const tokenSync = path.join(here, '..', 'token-sync', 'node_modules');

fs.mkdirSync(vendor, { recursive: true });

function copy(from, to) {
  fs.copyFileSync(from, path.join(vendor, to));
  console.log('copied', to);
}

function download(url, to) {
  return new Promise((resolve, reject) => {
    https.get(url, res => {
      if (res.statusCode !== 200) return reject(new Error(url + ' -> ' + res.statusCode));
      const chunks = [];
      res.on('data', c => chunks.push(c));
      res.on('end', () => {
        fs.writeFileSync(path.join(vendor, to), Buffer.concat(chunks));
        console.log('downloaded', to);
        resolve();
      });
    }).on('error', reject);
  });
}

copy(path.join(tokenSync, 'antd/dist/antd.min.js'), 'antd.min.js');
copy(path.join(tokenSync, 'dayjs/dayjs.min.js'), 'dayjs.min.js');

Promise.all([
  download('https://unpkg.com/react@18.3.1/umd/react.production.min.js', 'react.js'),
  download('https://unpkg.com/react-dom@18.3.1/umd/react-dom.production.min.js', 'react-dom.js')
]).catch(e => { console.error(e.message); process.exit(1); });

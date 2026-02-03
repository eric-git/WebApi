# syntax=docker/dockerfile:1

FROM lscr.io/linuxserver/webtop:debian-xfce AS jumpbox
USER root

# Debian sources list
RUN <<'EOF'
codename=$(awk -F= '/VERSION_CODENAME/{print $2}' /etc/os-release)
cat > /etc/apt/sources.list <<LIST
deb https://mirror.aarnet.edu.au/pub/debian $codename main contrib non-free non-free-firmware
deb https://mirror.aarnet.edu.au/pub/debian-security $codename-security main contrib non-free non-free-firmware
deb https://mirror.aarnet.edu.au/pub/debian $codename-updates main contrib non-free non-free-firmware
LIST
EOF

# Base packages
RUN --mount=type=cache,target=/var/cache/apt \
  --mount=type=cache,target=/var/lib/apt \
  apt-get update && \
  apt-get install -y \
  wget curl gnupg lsb-release ca-certificates && \
  rm -rf /var/lib/apt/lists/*

# ------------------------------------------------------------
# VS Code
# ------------------------------------------------------------
RUN --mount=type=cache,target=/var/cache/apt \
  --mount=type=cache,target=/var/lib/apt \
  set -eux; \
  \
  wget -qO- https://packages.microsoft.com/keys/microsoft.asc \
  | gpg --dearmor > /usr/share/keyrings/ms.gpg; \
  echo "deb [arch=amd64 signed-by=/usr/share/keyrings/ms.gpg] https://packages.microsoft.com/repos/code stable main" \
  > /etc/apt/sources.list.d/vscode.list; \
  \
  apt-get update; \
  apt-get install -y code; \
  rm -rf /var/lib/apt/lists/*; \
  \
  printf '#!/bin/bash\n/usr/share/code/code --no-sandbox "$@"\n' \
  > /usr/local/bin/code; \
  chmod +x /usr/local/bin/code; \
  ln -sf /usr/local/bin/code /usr/share/code/bin/code; \
  sed -i 's|Exec=/usr/share/code/code|Exec=/usr/local/bin/code|' \
  /usr/share/applications/code.desktop

# ------------------------------------------------------------
# pgAdmin
# ------------------------------------------------------------
RUN --mount=type=cache,target=/var/cache/apt \
  --mount=type=cache,target=/var/lib/apt \
  codename=$(awk -F= '/VERSION_CODENAME/{print $2}' /etc/os-release) && \
  curl -fsSL https://www.pgadmin.org/static/packages_pgadmin_org.pub \
  | gpg --dearmor -o /usr/share/keyrings/pgadmin.gpg && \
  echo "deb [signed-by=/usr/share/keyrings/pgadmin.gpg] \
  https://ftp.postgresql.org/pub/pgadmin/pgadmin4/apt/$codename pgadmin4 main" \
  > /etc/apt/sources.list.d/pgadmin4.list && \
  apt-get update && \
  apt-get install -y pgadmin4-desktop && \
  rm -rf /var/lib/apt/lists/* && \
  mv /usr/pgadmin4/bin/pgadmin4 /usr/pgadmin4/bin/pgadmin4.real && \
  printf '#!/bin/bash\n/usr/pgadmin4/bin/pgadmin4.real --no-sandbox --disable-seccomp-filter-sandbox "$@"\n' \
  > /usr/pgadmin4/bin/pgadmin4 && \
  chmod +x /usr/pgadmin4/bin/pgadmin4;

# ------------------------------------------------------------
# .NET SDK
# ------------------------------------------------------------
RUN --mount=type=cache,target=/var/cache/apt \
  --mount=type=cache,target=/var/lib/apt \
  set -eux; \
  ver=$(awk -F= '/VERSION_ID/{print $2}' /etc/os-release | tr -d '"'); \
  wget -q https://packages.microsoft.com/config/debian/$ver/packages-microsoft-prod.deb -O /tmp/ms.deb; \
  dpkg -i /tmp/ms.deb; \
  rm /tmp/ms.deb; \
  apt-get update; \
  apt-get install -y dotnet-sdk-10.0; \
  rm -rf /var/lib/apt/lists/*

# ------------------------------------------------------------
# NodeJS
# ------------------------------------------------------------
RUN --mount=type=cache,target=/var/cache/apt \
  --mount=type=cache,target=/var/lib/apt \
  set -eux; \
  curl -fsSL https://deb.nodesource.com/setup_lts.x | bash -; \
  apt-get update; \
  apt-get install -y nodejs; \
  rm -rf /var/lib/apt/lists/*

# ------------------------------------------------------------
# Add internal CA
# ------------------------------------------------------------
COPY --link local/hosting/api-cert.crt local/hosting/issuer-cert.crt /usr/local/share/ca-certificates/
RUN update-ca-certificates

# ------------------------------------------------------------
# Misc customizations
# ------------------------------------------------------------
COPY --link docker/jumpbox/wallpaper.jpg /usr/share/backgrounds/xfce/jumpbox-wallpaper.jpg
RUN chmod 644 /usr/share/backgrounds/xfce/jumpbox-wallpaper.jpg

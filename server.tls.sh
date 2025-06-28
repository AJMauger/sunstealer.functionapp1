# Kong/Dashboard
openssl genrsa -out tls.key 2048
openssl req -new -key tls.key -addext "subjectAltName = DNS:*.ajm.net" -out tls.csr -subj "/CN=*.ajm.net/C=US/ST=CO/L=Denver/O=ajm.net/OU=skubernetes-dashboard/emailAddress=adammauger@mail.com"
openssl x509 -req -days 10000 -CA ca-ajm.net.crt -CAkey ca-ajm.net.key -CAcreateserial  -in tls.csr -out tls.crt -extensions SAN -extfile <(printf "\n[SAN]\nsubjectAltName=DNS:*.ajm.net")
openssl x509 -noout -text -in tls.crt

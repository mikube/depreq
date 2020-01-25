<h1 align="center">:rocket: Depreq</h1>
Deploy Requester for GitOps.

Clone a manifest repository, update tags, commit the change, and create a pull request! (from CI context)


## :trident: Supported Options
### Template Engines
* Yaml
  * Helm
  * Kustomize
  * Plain kubernetes manifest
* Any
  * By regular expression

### Git Repositories
* GitHub


## :soon: Usage
Download a single binary from [release page](https://github.com/minikube/Depreq/releases).

```sh
# CI on application repository
depreq \
  --git-user amaya382 \
  --git-email mail@sapphire.in.net \
  --git-token <your-git-token> \
  --git-token-user amaya382 \
  --manifest-uri 'https://github.com/gitops-demo/manifests.git' \
  --manifest-base-branch dev \
  --manifest-values-files app-a/values/dev-app-a.yaml:app-a/values/prd-app-a.yaml \
  --manifest-tag-keys 'image.repository' \
  --app-image gitopsdemo/app-a \
  --app-curr-commit <commit-hash-from-ci> \
  --github-user amaya382
```

And also you can use [docker images](https://hub.docker.com/repository/docker/mikube/depreq).


### How to update tags
#### By key
You can use a brief query. `.` for map, `[*].` for array.

```yaml
# --manifest-tag-keys 'image.repository'
image:
  repository: gitops-demo/app-a:1234567890abcdef
```

```yaml
# --manifest-tag-keys 'tag'
image: gitops-demo/app-a
tag: 1234567890abcdef
```

```yaml
# --manifest-tag-keys 'spec.template.spec.containers[*].image'
spec:
  template:
    spec:
      containers:
      - name: app-1
        image: gitops-demo/app-a:1234567890abcdef
      - name: app-2
        image: gitops-demo/app-a:1234567890abcdef
```

#### By regular expression
If you want to update tags by regular expression,
use '--manifest-tag-pattern' instead of '--manifest-tag-keys'.

```yaml
# --manifest-tag-pattern 'repository: gitops-demo/app-a:[\da-f]+$'
image:
  repository: gitops-demo/app-a:1234567890abcdef
```

```yaml
# --manifest-tag-pattern 'tag: [\da-f]+$'
image: gitops-demo/app-a
tag: 1234567890abcdef
```

```yaml
# --manifest-tag-pattern 'image: gitops-demo/app-a:[\da-f]+$'
spec:
  template:
    spec:
      containers:
      - name: app-1
        image: gitops-demo/app-a:1234567890abcdef
      - name: app-2
        image: gitops-demo/app-a:1234567890abcdef
```


## :round_pushpin: Note
* Now Depreq uses a git commit hash as an image tag

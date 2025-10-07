/* eslint-disable */
/* tslint:disable */
// @ts-nocheck
/*
 * ---------------------------------------------------------------
 * ## THIS FILE WAS GENERATED VIA SWAGGER-TYPESCRIPT-API        ##
 * ##                                                           ##
 * ## AUTHOR: acacode                                           ##
 * ## SOURCE: https://github.com/acacode/swagger-typescript-api ##
 * ---------------------------------------------------------------
 */

export interface AnalysisResponseDto {
  success?: boolean;
  data?: any;
  message?: string | null;
  resultFormat?: string | null;
}

export interface CreateJenkinsServersRequest {
  name?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  username?: string | null;
  apiToken?: string | null;
}

export interface CreateKnowledgeBaseDto {
  name?: string | null;
  description?: string | null;
}

export interface CreateKubeClusterRequest {
  name?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  bearerToken?: string | null;
  certificateAuthorityPem?: string | null;
  insecureSkipTlsVerify?: boolean;
  defaultNamespace?: string | null;
}

export interface DeploymentAnalysisRequestDto {
  namespace?: string | null;
  deploymentName?: string | null;
  fallback?: boolean | null;
  includeDescription?: boolean | null;
}

export interface DeploymentLogRequest {
  namespace?: string | null;
  deploymentName?: string | null;
  fallback?: boolean | null;
  includeDescription?: boolean | null;
}

export interface JenkinsServersResponse {
  /** @format uuid */
  id?: string;
  name?: string | null;
  type?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
  username?: string | null;
}

export interface KnowledgeBaseDto {
  /** @format uuid */
  id?: string;
  name?: string | null;
  description?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface KnowledgeBaseWithSourcesDto {
  /** @format uuid */
  id?: string;
  name?: string | null;
  description?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
  sources?: SourceReadDto[] | null;
}

export interface KnowledgeFileDto {
  /** @format uuid */
  id?: string;
  /** @format uuid */
  knowledgeBaseId?: string;
  fileName?: string | null;
  contentType?: string | null;
  /** @format int64 */
  fileSize?: number;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface KubeClusterResponse {
  /** @format uuid */
  id?: string;
  name?: string | null;
  type?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
  hasBearerToken?: boolean;
  hasCertificateAuthority?: boolean;
  insecureSkipTlsVerify?: boolean;
  defaultNamespace?: string | null;
}

export interface LogMetadata {
  podName?: string | null;
  containerName?: string | null;
  namespace?: string | null;
  deploymentName?: string | null;
}

export interface LogResponse {
  logs?: string | null;
  description?: string | null;
  metadata?: LogMetadata;
}

export interface LoginRequest {
  email?: string | null;
  password?: string | null;
}

export interface PodAnalysisRequestDto {
  namespace?: string | null;
  podName?: string | null;
  containerName?: string | null;
  /** @format int32 */
  tailLines?: number | null;
  /** @format int32 */
  sinceSeconds?: number | null;
  previous?: boolean | null;
  /** @format int32 */
  limitBytes?: number | null;
  includeDescription?: boolean | null;
}

export interface PodLogRequest {
  namespace?: string | null;
  podName?: string | null;
  containerName?: string | null;
  /** @format int32 */
  tailLines?: number | null;
  /** @format int32 */
  sinceSeconds?: number | null;
  previous?: boolean | null;
  /** @format int32 */
  limitBytes?: number | null;
  includeDescription?: boolean | null;
}

export interface ProblemDetails {
  type?: string | null;
  title?: string | null;
  /** @format int32 */
  status?: number | null;
  detail?: string | null;
  instance?: string | null;
  [key: string]: any;
}

export interface RegisterRequest {
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  password?: string | null;
}

export interface SourceReadDto {
  /** @format uuid */
  id?: string;
  name?: string | null;
  type?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface UpdateJenkinsServersRequest {
  name?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  username?: string | null;
  apiToken?: string | null;
}

export interface UpdateKnowledgeBaseDto {
  /** @format uuid */
  id?: string;
  name?: string | null;
  description?: string | null;
}

export interface UpdateKubeClusterRequest {
  name?: string | null;
  server?: string | null;
  instructions?: string | null;
  responseFormat?: string | null;
  bearerToken?: string | null;
  certificateAuthorityPem?: string | null;
  insecureSkipTlsVerify?: boolean | null;
  defaultNamespace?: string | null;
}

export type QueryParamsType = Record<string | number, any>;
export type ResponseFormat = keyof Omit<Body, "body" | "bodyUsed">;

export interface FullRequestParams extends Omit<RequestInit, "body"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseFormat;
  /** request body */
  body?: unknown;
  /** base url */
  baseUrl?: string;
  /** request cancellation token */
  cancelToken?: CancelToken;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown> {
  baseUrl?: string;
  baseApiParams?: Omit<RequestParams, "baseUrl" | "cancelToken" | "signal">;
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<RequestParams | void> | RequestParams | void;
  customFetch?: typeof fetch;
}

export interface HttpResponse<D extends unknown, E extends unknown = unknown>
  extends Response {
  data: D;
  error: E;
}

type CancelToken = Symbol | string | number;

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public baseUrl: string = "";
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private abortControllers = new Map<CancelToken, AbortController>();
  private customFetch = (...fetchParams: Parameters<typeof fetch>) =>
    fetch(...fetchParams);

  private baseApiParams: RequestParams = {
    credentials: "same-origin",
    headers: {},
    redirect: "follow",
    referrerPolicy: "no-referrer",
  };

  constructor(apiConfig: ApiConfig<SecurityDataType> = {}) {
    Object.assign(this, apiConfig);
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected encodeQueryParam(key: string, value: any) {
    const encodedKey = encodeURIComponent(key);
    return `${encodedKey}=${encodeURIComponent(typeof value === "number" ? value : `${value}`)}`;
  }

  protected addQueryParam(query: QueryParamsType, key: string) {
    return this.encodeQueryParam(key, query[key]);
  }

  protected addArrayQueryParam(query: QueryParamsType, key: string) {
    const value = query[key];
    return value.map((v: any) => this.encodeQueryParam(key, v)).join("&");
  }

  protected toQueryString(rawQuery?: QueryParamsType): string {
    const query = rawQuery || {};
    const keys = Object.keys(query).filter(
      (key) => "undefined" !== typeof query[key],
    );
    return keys
      .map((key) =>
        Array.isArray(query[key])
          ? this.addArrayQueryParam(query, key)
          : this.addQueryParam(query, key),
      )
      .join("&");
  }

  protected addQueryParams(rawQuery?: QueryParamsType): string {
    const queryString = this.toQueryString(rawQuery);
    return queryString ? `?${queryString}` : "";
  }

  private contentFormatters: Record<ContentType, (input: any) => any> = {
    [ContentType.Json]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.JsonApi]: (input: any) =>
      input !== null && (typeof input === "object" || typeof input === "string")
        ? JSON.stringify(input)
        : input,
    [ContentType.Text]: (input: any) =>
      input !== null && typeof input !== "string"
        ? JSON.stringify(input)
        : input,
    [ContentType.FormData]: (input: any) => {
      if (input instanceof FormData) {
        return input;
      }

      return Object.keys(input || {}).reduce((formData, key) => {
        const property = input[key];
        formData.append(
          key,
          property instanceof Blob
            ? property
            : typeof property === "object" && property !== null
              ? JSON.stringify(property)
              : `${property}`,
        );
        return formData;
      }, new FormData());
    },
    [ContentType.UrlEncoded]: (input: any) => this.toQueryString(input),
  };

  protected mergeRequestParams(
    params1: RequestParams,
    params2?: RequestParams,
  ): RequestParams {
    return {
      ...this.baseApiParams,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...(this.baseApiParams.headers || {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected createAbortSignal = (
    cancelToken: CancelToken,
  ): AbortSignal | undefined => {
    if (this.abortControllers.has(cancelToken)) {
      const abortController = this.abortControllers.get(cancelToken);
      if (abortController) {
        return abortController.signal;
      }
      return void 0;
    }

    const abortController = new AbortController();
    this.abortControllers.set(cancelToken, abortController);
    return abortController.signal;
  };

  public abortRequest = (cancelToken: CancelToken) => {
    const abortController = this.abortControllers.get(cancelToken);

    if (abortController) {
      abortController.abort();
      this.abortControllers.delete(cancelToken);
    }
  };

  public request = async <T = any, E = any>({
    body,
    secure,
    path,
    type,
    query,
    format,
    baseUrl,
    cancelToken,
    ...params
  }: FullRequestParams): Promise<HttpResponse<T, E>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.baseApiParams.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const queryString = query && this.toQueryString(query);
    const payloadFormatter = this.contentFormatters[type || ContentType.Json];
    const responseFormat = format || requestParams.format;

    return this.customFetch(
      `${baseUrl || this.baseUrl || ""}${path}${queryString ? `?${queryString}` : ""}`,
      {
        ...requestParams,
        headers: {
          ...(requestParams.headers || {}),
          ...(type && type !== ContentType.FormData
            ? { "Content-Type": type }
            : {}),
        },
        signal:
          (cancelToken
            ? this.createAbortSignal(cancelToken)
            : requestParams.signal) || null,
        body:
          typeof body === "undefined" || body === null
            ? null
            : payloadFormatter(body),
      },
    ).then(async (response) => {
      const r = response as HttpResponse<T, E>;
      r.data = null as unknown as T;
      r.error = null as unknown as E;

      const responseToParse = responseFormat ? response.clone() : response;
      const data = !responseFormat
        ? r
        : await responseToParse[responseFormat]()
            .then((data) => {
              if (r.ok) {
                r.data = data;
              } else {
                r.error = data;
              }
              return r;
            })
            .catch((e) => {
              r.error = e;
              return r;
            });

      if (cancelToken) {
        this.abortControllers.delete(cancelToken);
      }

      if (!response.ok) throw data;
      return data;
    });
  };
}

/**
 * @title PodMD.Api
 * @version 1.0
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  api = {
    /**
     * No description
     *
     * @tags Analysis
     * @name V1ClustersAnalysisPodsCreate
     * @request POST:/api/v1/clusters/{clusterId}/Analysis/pods
     * @secure
     */
    v1ClustersAnalysisPodsCreate: (
      clusterId: string,
      data: PodAnalysisRequestDto,
      params: RequestParams = {},
    ) =>
      this.request<AnalysisResponseDto, ProblemDetails>({
        path: `/api/v1/clusters/${clusterId}/Analysis/pods`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Analysis
     * @name V1ClustersAnalysisDeploymentsCreate
     * @request POST:/api/v1/clusters/{clusterId}/Analysis/deployments
     * @secure
     */
    v1ClustersAnalysisDeploymentsCreate: (
      clusterId: string,
      data: DeploymentAnalysisRequestDto,
      params: RequestParams = {},
    ) =>
      this.request<AnalysisResponseDto, ProblemDetails>({
        path: `/api/v1/clusters/${clusterId}/Analysis/deployments`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name V1AuthRegisterCreate
     * @request POST:/api/v1/Auth/register
     * @secure
     */
    v1AuthRegisterCreate: (data: RegisterRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/Auth/register`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name V1AuthLoginCreate
     * @request POST:/api/v1/Auth/login
     * @secure
     */
    v1AuthLoginCreate: (data: LoginRequest, params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/Auth/login`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Auth
     * @name V1AuthMeList
     * @request GET:/api/v1/Auth/me
     * @secure
     */
    v1AuthMeList: (params: RequestParams = {}) =>
      this.request<void, any>({
        path: `/api/v1/Auth/me`,
        method: "GET",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clusters
     * @name V1ClustersCreate
     * @request POST:/api/v1/Clusters
     * @secure
     */
    v1ClustersCreate: (
      data: CreateKubeClusterRequest,
      params: RequestParams = {},
    ) =>
      this.request<KubeClusterResponse, ProblemDetails>({
        path: `/api/v1/Clusters`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clusters
     * @name V1ClustersList
     * @request GET:/api/v1/Clusters
     * @secure
     */
    v1ClustersList: (params: RequestParams = {}) =>
      this.request<KubeClusterResponse[], ProblemDetails>({
        path: `/api/v1/Clusters`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clusters
     * @name V1ClustersDetail
     * @request GET:/api/v1/Clusters/{id}
     * @secure
     */
    v1ClustersDetail: (id: string, params: RequestParams = {}) =>
      this.request<KubeClusterResponse, ProblemDetails>({
        path: `/api/v1/Clusters/${id}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clusters
     * @name V1ClustersUpdate
     * @request PUT:/api/v1/Clusters/{id}
     * @secure
     */
    v1ClustersUpdate: (
      id: string,
      data: UpdateKubeClusterRequest,
      params: RequestParams = {},
    ) =>
      this.request<KubeClusterResponse, ProblemDetails>({
        path: `/api/v1/Clusters/${id}`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Clusters
     * @name V1ClustersDelete
     * @request DELETE:/api/v1/Clusters/{id}
     * @secure
     */
    v1ClustersDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/Clusters/${id}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags JenkinsServers
     * @name V1JenkinsServersCreate
     * @request POST:/api/v1/jenkins-servers
     * @secure
     */
    v1JenkinsServersCreate: (
      data: CreateJenkinsServersRequest,
      params: RequestParams = {},
    ) =>
      this.request<JenkinsServersResponse, ProblemDetails>({
        path: `/api/v1/jenkins-servers`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags JenkinsServers
     * @name V1JenkinsServersList
     * @request GET:/api/v1/jenkins-servers
     * @secure
     */
    v1JenkinsServersList: (params: RequestParams = {}) =>
      this.request<JenkinsServersResponse[], ProblemDetails>({
        path: `/api/v1/jenkins-servers`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags JenkinsServers
     * @name V1JenkinsServersDetail
     * @request GET:/api/v1/jenkins-servers/{id}
     * @secure
     */
    v1JenkinsServersDetail: (id: string, params: RequestParams = {}) =>
      this.request<JenkinsServersResponse, ProblemDetails>({
        path: `/api/v1/jenkins-servers/${id}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags JenkinsServers
     * @name V1JenkinsServersUpdate
     * @request PUT:/api/v1/jenkins-servers/{id}
     * @secure
     */
    v1JenkinsServersUpdate: (
      id: string,
      data: UpdateJenkinsServersRequest,
      params: RequestParams = {},
    ) =>
      this.request<JenkinsServersResponse, ProblemDetails>({
        path: `/api/v1/jenkins-servers/${id}`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags JenkinsServers
     * @name V1JenkinsServersDelete
     * @request DELETE:/api/v1/jenkins-servers/{id}
     * @secure
     */
    v1JenkinsServersDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/jenkins-servers/${id}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesCreate
     * @request POST:/api/v1/knowledge-bases
     * @secure
     */
    v1KnowledgeBasesCreate: (
      data: CreateKnowledgeBaseDto,
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeBaseDto, ProblemDetails>({
        path: `/api/v1/knowledge-bases`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesList
     * @request GET:/api/v1/knowledge-bases
     * @secure
     */
    v1KnowledgeBasesList: (params: RequestParams = {}) =>
      this.request<KnowledgeBaseDto[], ProblemDetails>({
        path: `/api/v1/knowledge-bases`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesDetail
     * @request GET:/api/v1/knowledge-bases/{id}
     * @secure
     */
    v1KnowledgeBasesDetail: (id: string, params: RequestParams = {}) =>
      this.request<KnowledgeBaseWithSourcesDto, ProblemDetails>({
        path: `/api/v1/knowledge-bases/${id}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesUpdate
     * @request PUT:/api/v1/knowledge-bases/{id}
     * @secure
     */
    v1KnowledgeBasesUpdate: (
      id: string,
      data: UpdateKnowledgeBaseDto,
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeBaseDto, ProblemDetails>({
        path: `/api/v1/knowledge-bases/${id}`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesDelete
     * @request DELETE:/api/v1/knowledge-bases/{id}
     * @secure
     */
    v1KnowledgeBasesDelete: (id: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/knowledge-bases/${id}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeBases
     * @name V1KnowledgeBasesSourcesList
     * @request GET:/api/v1/knowledge-bases/{id}/sources
     * @secure
     */
    v1KnowledgeBasesSourcesList: (id: string, params: RequestParams = {}) =>
      this.request<SourceReadDto[], ProblemDetails>({
        path: `/api/v1/knowledge-bases/${id}/sources`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeFile
     * @name V1FilesReplaceUpdate
     * @request PUT:/api/v1/files/{fileId}/replace
     * @secure
     */
    v1FilesReplaceUpdate: (
      fileId: string,
      data: {
        /** @format binary */
        file?: File;
      },
      query?: {
        newFileName?: string;
      },
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeFileDto, ProblemDetails>({
        path: `/api/v1/files/${fileId}/replace`,
        method: "PUT",
        query: query,
        body: data,
        secure: true,
        type: ContentType.FormData,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeFile
     * @name V1FilesDelete
     * @request DELETE:/api/v1/files/{fileId}
     * @secure
     */
    v1FilesDelete: (fileId: string, params: RequestParams = {}) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/files/${fileId}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeFiles
     * @name V1KnowledgebasesFilesCreate
     * @request POST:/api/v1/knowledgebases/{knowledgeBaseId}/files
     * @secure
     */
    v1KnowledgebasesFilesCreate: (
      knowledgeBaseId: string,
      data: {
        files?: File[];
      },
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeFileDto[], ProblemDetails>({
        path: `/api/v1/knowledgebases/${knowledgeBaseId}/files`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.FormData,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags KnowledgeFiles
     * @name V1KnowledgebasesFilesList
     * @request GET:/api/v1/knowledgebases/{knowledgeBaseId}/files
     * @secure
     */
    v1KnowledgebasesFilesList: (
      knowledgeBaseId: string,
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeFileDto[], ProblemDetails>({
        path: `/api/v1/knowledgebases/${knowledgeBaseId}/files`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Logs
     * @name V1ClustersLogsPodsCreate
     * @request POST:/api/v1/clusters/{clusterId}/Logs/pods
     * @secure
     */
    v1ClustersLogsPodsCreate: (
      clusterId: string,
      data: PodLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<LogResponse, ProblemDetails>({
        path: `/api/v1/clusters/${clusterId}/Logs/pods`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Logs
     * @name V1ClustersLogsDeploymentsCreate
     * @request POST:/api/v1/clusters/{clusterId}/Logs/deployments
     * @secure
     */
    v1ClustersLogsDeploymentsCreate: (
      clusterId: string,
      data: DeploymentLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<LogResponse, ProblemDetails>({
        path: `/api/v1/clusters/${clusterId}/Logs/deployments`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name V1SourcesKnowledgeBasesList
     * @request GET:/api/v1/sources/{sourceId}/knowledge-bases
     * @secure
     */
    v1SourcesKnowledgeBasesList: (
      sourceId: string,
      params: RequestParams = {},
    ) =>
      this.request<KnowledgeBaseDto[], ProblemDetails>({
        path: `/api/v1/sources/${sourceId}/knowledge-bases`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name V1SourcesKnowledgeBasesCreate
     * @request POST:/api/v1/sources/{sourceId}/knowledge-bases/{knowledgeBaseId}
     * @secure
     */
    v1SourcesKnowledgeBasesCreate: (
      sourceId: string,
      knowledgeBaseId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/sources/${sourceId}/knowledge-bases/${knowledgeBaseId}`,
        method: "POST",
        secure: true,
        ...params,
      }),

    /**
     * No description
     *
     * @tags Sources
     * @name V1SourcesKnowledgeBasesDelete
     * @request DELETE:/api/v1/sources/{sourceId}/knowledge-bases/{knowledgeBaseId}
     * @secure
     */
    v1SourcesKnowledgeBasesDelete: (
      sourceId: string,
      knowledgeBaseId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, ProblemDetails>({
        path: `/api/v1/sources/${sourceId}/knowledge-bases/${knowledgeBaseId}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),
  };
}

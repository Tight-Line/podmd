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

/**
 * AnalysisResponse
 * Response model for log analysis.
 */
export interface AnalysisResponse {
  /** Success */
  success: boolean;
  /** Message */
  message: string;
  /** Data */
  data?: object | null;
}

/**
 * DeploymentLogRequest
 * Request for retrieving logs from failed pods in a deployment.
 */
export interface DeploymentLogRequest {
  /**
   * Namespace
   * Kubernetes namespace
   */
  namespace: string;
  /**
   * Deployment Name
   * Name of the deployment
   */
  deployment_name: string;
  /**
   * Fallback
   * Fallback to any pod if no failed pods found
   * @default false
   */
  fallback?: boolean | null;
}

/** HTTPValidationError */
export interface HTTPValidationError {
  /** Detail */
  detail?: ValidationError[];
}

/**
 * KubeClusterCreate
 * Schema for creating a new Kubernetes cluster with base source fields.
 */
export interface KubeClusterCreate {
  /**
   * Name
   * @minLength 1
   * @maxLength 100
   */
  name: string;
  /**
   * Server
   * Kubernetes API server URL
   */
  server: string;
  /**
   * Bearer Token
   * Bearer token for cluster authentication
   */
  bearer_token: string;
  /**
   * Certificate Authority Pem
   * CA certificate PEM
   */
  certificate_authority_pem?: string | null;
  /**
   * Insecure Skip Tls Verify
   * Skip TLS verification
   * @default false
   */
  insecure_skip_tls_verify?: boolean;
  /**
   * Default Namespace
   * Default Kubernetes namespace
   */
  default_namespace?: string | null;
  /**
   * Instructions
   * Instructions for AI processing
   */
  instructions?: string | null;
  /**
   * Response Format
   * Response format for AI
   */
  response_format?: string | null;
}

/**
 * KubeClusterListResponse
 * Schema for Kubernetes cluster list responses.
 */
export interface KubeClusterListResponse {
  /** Kube Clusters */
  kube_clusters: KubeClusterResponse[];
}

/**
 * KubeClusterResponse
 * Schema for Kubernetes cluster response data - combined Source and KubeCluster fields.
 */
export interface KubeClusterResponse {
  /**
   * Id
   * @format uuid
   */
  id: string;
  /** Type */
  type: string;
  /** Name */
  name: string;
  /** Server */
  server: string;
  /** Key Version */
  key_version: number;
  /** Instructions */
  instructions?: string | null;
  /** Response Format */
  response_format?: string | null;
  /**
   * User Id
   * @format uuid
   */
  user_id: string;
  /**
   * Created At
   * @format date-time
   */
  created_at: string;
  /** Updated At */
  updated_at?: string | null;
  /** Bearer Token Enc */
  bearer_token_enc: string;
  /** Certificate Authority Pem */
  certificate_authority_pem?: string | null;
  /**
   * Insecure Skip Tls Verify
   * @default false
   */
  insecure_skip_tls_verify?: boolean;
  /** Default Namespace */
  default_namespace?: string | null;
}

/**
 * KubeClusterUpdate
 * Schema for updating an existing Kubernetes cluster.
 */
export interface KubeClusterUpdate {
  /** Name */
  name?: string | null;
  /** Server */
  server?: string | null;
  /** Key Version */
  key_version?: number | null;
  /** Instructions */
  instructions?: string | null;
  /** Response Format */
  response_format?: string | null;
  /** Bearer Token */
  bearer_token?: string | null;
  /** Certificate Authority Pem */
  certificate_authority_pem?: string | null;
  /** Insecure Skip Tls Verify */
  insecure_skip_tls_verify?: boolean | null;
  /** Default Namespace */
  default_namespace?: string | null;
}

/**
 * LogMetadata
 * Metadata about the log source.
 */
export interface LogMetadata {
  /** Pod Name */
  pod_name: string;
  /** Container Name */
  container_name: string | null;
  /** Namespace */
  namespace: string;
  /** Deployment Name */
  deployment_name: string | null;
}

/**
 * LogResponse
 * Response containing logs and metadata.
 */
export interface LogResponse {
  /** Logs */
  logs: string;
  /** Description */
  description: string;
  /** Metadata about the log source. */
  metadata: LogMetadata;
}

/**
 * PodLogRequest
 * Request for retrieving logs from a specific pod.
 */
export interface PodLogRequest {
  /**
   * Namespace
   * Kubernetes namespace
   */
  namespace: string;
  /**
   * Pod Name
   * Name of the pod
   */
  pod_name: string;
  /**
   * Container Name
   * Container name (optional)
   */
  container_name?: string | null;
  /**
   * Tail Lines
   * Number of lines to retrieve from end
   */
  tail_lines?: number | null;
  /**
   * Since Seconds
   * Return logs newer than this many seconds
   */
  since_seconds?: number | null;
  /**
   * Previous
   * Return previous terminated container logs
   * @default false
   */
  previous?: boolean | null;
  /**
   * Limit Bytes
   * Maximum bytes of logs to return
   */
  limit_bytes?: number | null;
}

/**
 * Token
 * Schema for JWT token response.
 */
export interface Token {
  /** Access Token */
  access_token: string;
  /**
   * Token Type
   * @default "bearer"
   */
  token_type?: string;
}

/**
 * UserCreate
 * Schema for user registration.
 */
export interface UserCreate {
  /**
   * Email
   * @format email
   */
  email: string;
  /** Password */
  password: string;
}

/**
 * UserLogin
 * Schema for user login.
 */
export interface UserLogin {
  /**
   * Email
   * @format email
   */
  email: string;
  /** Password */
  password: string;
}

/**
 * UserResponse
 * Schema for user response data.
 */
export interface UserResponse {
  /**
   * Email
   * @format email
   */
  email: string;
  /**
   * Id
   * @format uuid
   */
  id: string;
  /** Is Active */
  is_active: boolean;
  /**
   * Created At
   * @format date-time
   */
  created_at: string;
  /** Updated At */
  updated_at?: string | null;
}

/** ValidationError */
export interface ValidationError {
  /** Location */
  loc: (string | number)[];
  /** Message */
  msg: string;
  /** Error Type */
  type: string;
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
 * @title PodMD Python Backend
 * @version 0.1.0
 *
 * Experimental Python implementation using FastAPI and PostgreSQL with user authentication
 */
export class Api<
  SecurityDataType extends unknown,
> extends HttpClient<SecurityDataType> {
  /**
   * @description Root endpoint providing basic API information. Returns: dict: Welcome message and available endpoints
   *
   * @name RootGet
   * @summary Root
   * @request GET:/
   */
  rootGet = (params: RequestParams = {}) =>
    this.request<any, any>({
      path: `/`,
      method: "GET",
      format: "json",
      ...params,
    });

  auth = {
    /**
     * @description Register a new user.
     *
     * @tags authentication
     * @name RegisterUserAuthRegisterPost
     * @summary Register User
     * @request POST:/auth/register
     */
    registerUserAuthRegisterPost: (
      data: UserCreate,
      params: RequestParams = {},
    ) =>
      this.request<UserResponse, HTTPValidationError>({
        path: `/auth/register`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Login and return JWT access token.
     *
     * @tags authentication
     * @name LoginForAccessTokenAuthLoginPost
     * @summary Login For Access Token
     * @request POST:/auth/login
     */
    loginForAccessTokenAuthLoginPost: (
      data: UserLogin,
      params: RequestParams = {},
    ) =>
      this.request<Token, HTTPValidationError>({
        path: `/auth/login`,
        method: "POST",
        body: data,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  users = {
    /**
     * @description Get current user profile.
     *
     * @tags users
     * @name ReadUsersMeUsersMeGet
     * @summary Read Users Me
     * @request GET:/users/me
     * @secure
     */
    readUsersMeUsersMeGet: (params: RequestParams = {}) =>
      this.request<UserResponse, any>({
        path: `/users/me`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),
  };
  kubeClusters = {
    /**
     * @description List all Kubernetes clusters for the current user.
     *
     * @tags kube-clusters
     * @name ListKubeClustersKubeClustersGet
     * @summary List Kube Clusters
     * @request GET:/kube-clusters/
     * @secure
     */
    listKubeClustersKubeClustersGet: (params: RequestParams = {}) =>
      this.request<KubeClusterListResponse, any>({
        path: `/kube-clusters/`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * @description Create a new Kubernetes cluster with source configuration.
     *
     * @tags kube-clusters
     * @name CreateKubeClusterKubeClustersPost
     * @summary Create Kube Cluster
     * @request POST:/kube-clusters/
     * @secure
     */
    createKubeClusterKubeClustersPost: (
      data: KubeClusterCreate,
      params: RequestParams = {},
    ) =>
      this.request<KubeClusterResponse, HTTPValidationError>({
        path: `/kube-clusters/`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Get a specific Kubernetes cluster by ID.
     *
     * @tags kube-clusters
     * @name GetKubeClusterKubeClustersClusterIdGet
     * @summary Get Kube Cluster
     * @request GET:/kube-clusters/{cluster_id}
     * @secure
     */
    getKubeClusterKubeClustersClusterIdGet: (
      clusterId: string,
      params: RequestParams = {},
    ) =>
      this.request<KubeClusterResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}`,
        method: "GET",
        secure: true,
        format: "json",
        ...params,
      }),

    /**
     * @description Update an existing Kubernetes cluster.
     *
     * @tags kube-clusters
     * @name UpdateKubeClusterKubeClustersClusterIdPut
     * @summary Update Kube Cluster
     * @request PUT:/kube-clusters/{cluster_id}
     * @secure
     */
    updateKubeClusterKubeClustersClusterIdPut: (
      clusterId: string,
      data: KubeClusterUpdate,
      params: RequestParams = {},
    ) =>
      this.request<KubeClusterResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}`,
        method: "PUT",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Delete a Kubernetes cluster.
     *
     * @tags kube-clusters
     * @name DeleteKubeClusterKubeClustersClusterIdDelete
     * @summary Delete Kube Cluster
     * @request DELETE:/kube-clusters/{cluster_id}
     * @secure
     */
    deleteKubeClusterKubeClustersClusterIdDelete: (
      clusterId: string,
      params: RequestParams = {},
    ) =>
      this.request<void, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}`,
        method: "DELETE",
        secure: true,
        ...params,
      }),

    /**
     * @description Retrieve logs from a specific pod. Requires authentication and cluster ownership.
     *
     * @tags kube-cluster-logs
     * @name GetPodLogsKubeClustersClusterIdLogsPodsPost
     * @summary Get Pod Logs
     * @request POST:/kube-clusters/{cluster_id}/logs/pods
     * @secure
     */
    getPodLogsKubeClustersClusterIdLogsPodsPost: (
      clusterId: string,
      data: PodLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<LogResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}/logs/pods`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Retrieve logs from failed pods in a deployment. Finds failed pods (not Running/Succeeded) in the specified deployment and returns logs from the first failed pod. Requires authentication and cluster ownership.
     *
     * @tags kube-cluster-logs
     * @name GetDeploymentLogsKubeClustersClusterIdLogsDeploymentsPost
     * @summary Get Deployment Logs
     * @request POST:/kube-clusters/{cluster_id}/logs/deployments
     * @secure
     */
    getDeploymentLogsKubeClustersClusterIdLogsDeploymentsPost: (
      clusterId: string,
      data: DeploymentLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<LogResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}/logs/deployments`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Analyze logs from a specific pod using LLM. Retrieves logs from the pod and uses OpenAI to identify errors and provide solutions. Requires authentication and cluster ownership.
     *
     * @tags kube-cluster-analysis
     * @name AnalyzePodLogsKubeClustersClusterIdAnalyzePodsPost
     * @summary Analyze Pod Logs
     * @request POST:/kube-clusters/{cluster_id}/analyze/pods
     * @secure
     */
    analyzePodLogsKubeClustersClusterIdAnalyzePodsPost: (
      clusterId: string,
      data: PodLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<AnalysisResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}/analyze/pods`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),

    /**
     * @description Analyze logs from failed pods in a deployment using LLM. Finds failed pods, retrieves their logs, and uses OpenAI to identify errors and provide solutions. Requires authentication and cluster ownership.
     *
     * @tags kube-cluster-analysis
     * @name AnalyzeDeploymentLogsKubeClustersClusterIdAnalyzeDeploymentsPost
     * @summary Analyze Deployment Logs
     * @request POST:/kube-clusters/{cluster_id}/analyze/deployments
     * @secure
     */
    analyzeDeploymentLogsKubeClustersClusterIdAnalyzeDeploymentsPost: (
      clusterId: string,
      data: DeploymentLogRequest,
      params: RequestParams = {},
    ) =>
      this.request<AnalysisResponse, HTTPValidationError>({
        path: `/kube-clusters/${clusterId}/analyze/deployments`,
        method: "POST",
        body: data,
        secure: true,
        type: ContentType.Json,
        format: "json",
        ...params,
      }),
  };
  health = {
    /**
     * @description Health check endpoint returning system status. Returns: dict: Health status with timestamp and service info
     *
     * @name HealthCheckHealthGet
     * @summary Health Check
     * @request GET:/health
     */
    healthCheckHealthGet: (params: RequestParams = {}) =>
      this.request<any, any>({
        path: `/health`,
        method: "GET",
        format: "json",
        ...params,
      }),
  };
}

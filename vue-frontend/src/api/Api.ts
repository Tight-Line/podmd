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

export interface AnalysisDataDto {
  errors?: LogErrorDto[] | null;
}

export interface AnalysisResponseDto {
  success?: boolean;
  data?: AnalysisDataDto;
  message?: string | null;
}

export interface CreateKubeClusterRequest {
  name?: string | null;
  server?: string | null;
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

export interface KubeClusterResponse {
  /** @format uuid */
  id?: string;
  name?: string | null;
  server?: string | null;
  hasBearerToken?: boolean;
  hasCertificateAuthority?: boolean;
  insecureSkipTlsVerify?: boolean;
  defaultNamespace?: string | null;
  /** @format date-time */
  createdAt?: string;
  /** @format date-time */
  updatedAt?: string;
}

export interface LogErrorDto {
  generalMessage?: string | null;
  occurrences?: string[] | null;
  solutions?: SolutionDto[] | null;
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

export interface SolutionDto {
  description?: string | null;
  steps?: StepDto[] | null;
}

export interface StepDto {
  title?: string | null;
  explanation?: string | null;
  command?: string | null;
}

export interface UpdateKubeClusterRequest {
  name?: string | null;
  server?: string | null;
  bearerToken?: string | null;
  certificateAuthorityPem?: string | null;
  insecureSkipTlsVerify?: boolean | null;
  defaultNamespace?: string | null;
}

import type {
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  HeadersDefaults,
  ResponseType,
} from "axios";
import axios from "axios";

export type QueryParamsType = Record<string | number, any>;

export interface FullRequestParams
  extends Omit<AxiosRequestConfig, "data" | "params" | "url" | "responseType"> {
  /** set parameter to `true` for call `securityWorker` for this request */
  secure?: boolean;
  /** request path */
  path: string;
  /** content type of request body */
  type?: ContentType;
  /** query params */
  query?: QueryParamsType;
  /** format of response (i.e. response.json() -> format: "json") */
  format?: ResponseType;
  /** request body */
  body?: unknown;
}

export type RequestParams = Omit<
  FullRequestParams,
  "body" | "method" | "query" | "path"
>;

export interface ApiConfig<SecurityDataType = unknown>
  extends Omit<AxiosRequestConfig, "data" | "cancelToken"> {
  securityWorker?: (
    securityData: SecurityDataType | null,
  ) => Promise<AxiosRequestConfig | void> | AxiosRequestConfig | void;
  secure?: boolean;
  format?: ResponseType;
}

export enum ContentType {
  Json = "application/json",
  JsonApi = "application/vnd.api+json",
  FormData = "multipart/form-data",
  UrlEncoded = "application/x-www-form-urlencoded",
  Text = "text/plain",
}

export class HttpClient<SecurityDataType = unknown> {
  public instance: AxiosInstance;
  private securityData: SecurityDataType | null = null;
  private securityWorker?: ApiConfig<SecurityDataType>["securityWorker"];
  private secure?: boolean;
  private format?: ResponseType;

  constructor({
    securityWorker,
    secure,
    format,
    ...axiosConfig
  }: ApiConfig<SecurityDataType> = {}) {
    this.instance = axios.create({
      ...axiosConfig,
      baseURL: axiosConfig.baseURL || "",
    });
    this.secure = secure;
    this.format = format;
    this.securityWorker = securityWorker;
  }

  public setSecurityData = (data: SecurityDataType | null) => {
    this.securityData = data;
  };

  protected mergeRequestParams(
    params1: AxiosRequestConfig,
    params2?: AxiosRequestConfig,
  ): AxiosRequestConfig {
    const method = params1.method || (params2 && params2.method);

    return {
      ...this.instance.defaults,
      ...params1,
      ...(params2 || {}),
      headers: {
        ...((method &&
          this.instance.defaults.headers[
            method.toLowerCase() as keyof HeadersDefaults
          ]) ||
          {}),
        ...(params1.headers || {}),
        ...((params2 && params2.headers) || {}),
      },
    };
  }

  protected stringifyFormItem(formItem: unknown) {
    if (typeof formItem === "object" && formItem !== null) {
      return JSON.stringify(formItem);
    } else {
      return `${formItem}`;
    }
  }

  protected createFormData(input: Record<string, unknown>): FormData {
    if (input instanceof FormData) {
      return input;
    }
    return Object.keys(input || {}).reduce((formData, key) => {
      const property = input[key];
      const propertyContent: any[] =
        property instanceof Array ? property : [property];

      for (const formItem of propertyContent) {
        const isFileType = formItem instanceof Blob || formItem instanceof File;
        formData.append(
          key,
          isFileType ? formItem : this.stringifyFormItem(formItem),
        );
      }

      return formData;
    }, new FormData());
  }

  public request = async <T = any, _E = any>({
    secure,
    path,
    type,
    query,
    format,
    body,
    ...params
  }: FullRequestParams): Promise<AxiosResponse<T>> => {
    const secureParams =
      ((typeof secure === "boolean" ? secure : this.secure) &&
        this.securityWorker &&
        (await this.securityWorker(this.securityData))) ||
      {};
    const requestParams = this.mergeRequestParams(params, secureParams);
    const responseFormat = format || this.format || undefined;

    if (
      type === ContentType.FormData &&
      body &&
      body !== null &&
      typeof body === "object"
    ) {
      body = this.createFormData(body as Record<string, unknown>);
    }

    if (
      type === ContentType.Text &&
      body &&
      body !== null &&
      typeof body !== "string"
    ) {
      body = JSON.stringify(body);
    }

    return this.instance.request({
      ...requestParams,
      headers: {
        ...(requestParams.headers || {}),
        ...(type ? { "Content-Type": type } : {}),
      },
      params: query,
      responseType: responseFormat,
      data: body,
      url: path,
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
  };
}
